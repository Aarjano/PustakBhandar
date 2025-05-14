using System.ComponentModel.DataAnnotations;

namespace FinalProject.ViewModels
{
    /// <summary>
    /// ViewModel for the Member Registration form input.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// User's membership ID
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 3)]
        [Display(Name = "Membership ID")]
        public required string MembershipId { get; set; }

        /// <summary>
        /// User's first name
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        [Display(Name = "First Name")]
        public required string FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public required string LastName { get; set; }

        /// <summary>
        /// User's email address
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public required string Email { get; set; }

        /// <summary>
        /// User's password
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public required string Password { get; set; }

        /// <summary>
        /// Confirmation of user's password
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public required string ConfirmPassword { get; set; }
    }
}
