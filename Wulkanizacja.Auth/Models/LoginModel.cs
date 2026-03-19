using System.ComponentModel.DataAnnotations;

namespace Wulkanizacja.Auth.Models
{
    public class LoginModel
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
