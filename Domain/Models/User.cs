using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Domain.Models
{
    [DataContract]
    public class User
    {
        [DataMember (Name = "id")]
        public Guid Id { get; set; }

        [DataMember(Name = "userName")]
        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [DataMember(Name = "passwordHash")]
        [Required]
        public string PasswordHash { get; set; }

        [DataMember(Name = "email")]
        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [DataMember(Name = "createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataMember(Name = "lastLoginAt")]
        public DateTime lastLoginAt { get; set; }

        [JsonIgnore]
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
