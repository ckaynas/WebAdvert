using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAdvert.Web.Models.Accounts
{
    public class ForgotConfirmModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage ="Verification Code Required")]
        [Display(Name ="VerificationCode")]
        public string VerificationCode { get; set; }

        [Required(ErrorMessage ="New Password Required")]
        [Display(Name ="NewPassword")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}
