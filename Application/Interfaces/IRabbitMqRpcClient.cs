namespace Application.Interfaces
{
    public interface IRabbitMqRpcClient<TRequest, TResponse>
    {
        Task<TResponse> SendRequestAsync(string queueName, TRequest request, int timeoutSeconds = 10, CancellationToken cancellationToken = default);
    }
}