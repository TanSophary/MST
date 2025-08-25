using System.ComponentModel.DataAnnotations;

namespace MST.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage ="Email is required.")]
        [EmailAddress]
        public string Email { set; get; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { set; get; }
        public bool RememberMe { set; get;}
    }
}
