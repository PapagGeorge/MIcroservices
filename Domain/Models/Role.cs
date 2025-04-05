using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Domain.Models
{
    [DataContract]
    public class Role
    {
        [DataMember(Name = "id")]
        public Guid Id { get; set; }

        [DataMember(Name = "name")]
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
