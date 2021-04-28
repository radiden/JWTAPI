using System.ComponentModel.DataAnnotations;

namespace jwtapi.Models
{
    public class RegistrationModel
    {
        [Required(ErrorMessage = "Username is required!")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        public string Password { get; set; }

        [Required(ErrorMessage = "FullName is required!")]
        public string FullName { get; set; }
    }
}