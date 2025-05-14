using System.ComponentModel.DataAnnotations;

namespace FinalProject.ViewModels
{
    /// <summary>
    /// ViewModel for Admin login credentials
    /// </summary>
    public class AdminLoginViewModel
    {
        /// <summary>
        /// Admin username
        /// </summary>
        [Required]
        [Display(Name = "Username")]
        public required string Username { get; set; }

        /// <summary>
        /// Admin password
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public required string Password { get; set; }
        
        /// <summary>
        /// Whether to remember the admin login
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
} 