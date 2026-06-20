using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class UserRegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(3)]
        public string Username { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;
    }
}
