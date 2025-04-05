using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Domain.Models
{
    [DataContract]
    public class UserRole
    {
        [DataMember(Name = "userId")]
        public Guid UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        [DataMember(Name = "roleId")]
        public Guid RoleId { get; set; }

        [JsonIgnore]
        public Role Role { get; set; }
    }
}
