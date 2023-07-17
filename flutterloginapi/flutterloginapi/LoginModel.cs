using System.ComponentModel.DataAnnotations;

namespace flutterloginapi
{
    public class LoginModel
    {
        [Required]
        public string username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string password { get; set; }
    }
}
