using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wulkanizacja.Auth.PostgreSQL.Entities
{
    public class UserRecord
    {
        [Key]
        public Guid UserId { get; init; } = Guid.NewGuid();
        [Required]
        public string Name { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Email { get; set; }
    }
}
