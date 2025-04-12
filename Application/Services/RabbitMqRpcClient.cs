using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class RabbitMqRpcClient<TRequest, TResponse> : IRabbitMqRpcClient<TRequest, TResponse>, IDisposable
    {
        private readonly ILogger<RabbitMqRpcClient<TRequest, TResponse>> _logger;
        private readonly string _host;
        private readonly string _username;
        private readonly string _password;
        private readonly int _port;
        private IConnection _connection;
        private IModel _channel;
        private string _replyQueueName;
        private EventingBasicConsumer _consumer;
        private readonly object _lock = new object();
        private bool _disposed;

        public RabbitMqRpcClient(
            ILogger<RabbitMqRpcClient<TRequest, TResponse>> logger,
            string host = "localhost",
            string username = "guest",
            string password = "guest",
            int port = 5672)
        {
            _logger = logger;
            _host = host;
            _username = username;
            _password = password;
            _port = port;
            InitializeClient();
        }

        private void InitializeClient()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _host,
                    UserName = _username,
                    Password = _password,
                    Port = _port,
                    // Add a reasonable timeout for connection attempts
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
                    // Enable automatic recovery
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(30)
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Set QoS prefetch to limit concurrent processing
                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                _replyQueueName = _channel.QueueDeclare(queue: "",
                    durable: false,
                    exclusive: true,
                    autoDelete: true,
                    arguments: null).QueueName;

                _consumer = new EventingBasicConsumer(_channel);
                _channel.BasicConsume(
                    queue: _replyQueueName,
                    autoAck: true,
                    consumer: _consumer);

                _logger.LogInformation("RabbitMQ RPC client initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ RPC client");
                Dispose();
                throw;
            }
        }

        public async Task<TResponse> SendRequestAsync(string queueName, TRequest request, int timeoutSeconds = 30,
            CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(RabbitMqRpcClient<TRequest, TResponse>));
            }

            var correlationId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<TResponse>();

            try
            {
                // Reconnect if channel is closed
                if (_channel == null || _channel.IsClosed)
                {
                    lock (_lock)
                    {
                        if (_channel == null || _channel.IsClosed)
                        {
                            InitializeClient();
                        }
                    }
                }

                // Declare the queue to ensure it exists
                _channel.QueueDeclare(queue: queueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                // Add handler for received messages
                EventHandler<BasicDeliverEventArgs> handler = null;
                handler = (model, ea) =>
                {
                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        try
                        {
                            var responseJson = Encoding.UTF8.GetString(ea.Body.ToArray());
                            var response = JsonSerializer.Deserialize<TResponse>(responseJson, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                            _logger.LogInformation("Received response for CorrelationId: {CorrelationId}",
                                correlationId);
                            tcs.TrySetResult(response);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error deserializing response");
                            tcs.TrySetException(ex);
                        }
                    }
                };

                _consumer.Received += handler;

                // Prepare message properties
                var properties = _channel.CreateBasicProperties();
                properties.CorrelationId = correlationId;
                properties.ReplyTo = _replyQueueName;
                properties.Expiration = (timeoutSeconds * 1000).ToString(); // Message TTL in milliseconds

                // Serialize and send message
                var messageJson = JsonSerializer.Serialize(request);
                var body = Encoding.UTF8.GetBytes(messageJson);

                _logger.LogInformation("Sending request to queue '{Queue}' with CorrelationId: {CorrelationId}",
                    queueName, correlationId);

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: queueName,
                    basicProperties: properties,
                    body: body
                );

                // Set up timeout
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

                await using (timeoutCts.Token.Register(() => tcs.TrySetCanceled()))
                {
                    try
                    {
                        return await tcs.Task.ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        _logger.LogError("RPC call to queue '{Queue}' timed out after {Timeout}s", queueName,
                            timeoutSeconds);
                        throw new TimeoutException(
                            $"RPC call to '{queueName}' timed out after {timeoutSeconds} seconds.");
                    }
                    finally
                    {
                        // Unsubscribe from the event
                        _consumer.Received -= handler;
                    }
                }
            }
            catch (Exception ex) when (!(ex is TaskCanceledException || ex is TimeoutException))
            {
                _logger.LogError(ex, "Error during RPC call to queue '{Queue}'", queueName);
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ client resources");
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}