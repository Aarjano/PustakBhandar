using System.ComponentModel.DataAnnotations;

namespace FinalProject.ViewModels
{
    /// <summary>
    /// ViewModel for user login credentials
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// User email
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public required string Email { get; set; }

        /// <summary>
        /// User password
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public required string Password { get; set; }

        /// <summary>
        /// Whether to remember the user's login
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
} 