using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace OktaAPIShared.Models
{
    public class Login
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "User Name")]
        [EmailAddress]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public bool IsAuthenticated { get; set; }

        public LoginViewModel()
        {
            IsAuthenticated = false;
        }
    }

    public class SMSViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "Code")]
        public string PassCode { get; set; }

        public string ApiUrl { get; set; }

        [Required]
        [Display(Name = "Country Code")]
        public string CountryCode { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; }
    }
}
