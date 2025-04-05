using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Domain.DTOs
{
    [DataContract]
    public class RegisterRequest
    {
        [DataMember(Name = "userName")]
        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [DataMember(Name = "email")]
        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [DataMember(Name = "password")]
        [Required]
        [StringLength(50)]
        public string Password { get; set; }
    }
}
