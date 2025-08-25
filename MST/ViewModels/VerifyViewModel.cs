using System.ComponentModel.DataAnnotations;

namespace MST.ViewModels
{
    public class VerifyViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
