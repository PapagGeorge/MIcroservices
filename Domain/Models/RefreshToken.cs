using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Domain.Models
{
    [DataContract]
    public class RefreshToken
    {
        [DataMember(Name = "id")]
        public Guid Id { get; set; }

        [DataMember(Name = "token")]
        public string Token { get; set; }

        [DataMember(Name = "expires")]
        public DateTime Expires { get; set; }

        [DataMember(Name = "isExpired")]
        public bool IsExpired => DateTime.UtcNow >= Expires;

        [DataMember(Name = "userId")]
        public Guid UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; }
    }
}
