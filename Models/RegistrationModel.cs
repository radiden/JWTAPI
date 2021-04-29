using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace jwtapi.Models
{
    public class RegistrationModel
    {
        // [JsonPropertyName("userName")]
        [Required(ErrorMessage = "Username is required!")]
        public string UserName { get; set; }
        // [JsonPropertyName("password")]
        [Required(ErrorMessage = "Password is required!")]
        public string Password { get; set; }
        // [JsonPropertyName("fullName")]
        [Required(ErrorMessage = "FullName is required!")]
        public string FullName { get; set; }
    }
}