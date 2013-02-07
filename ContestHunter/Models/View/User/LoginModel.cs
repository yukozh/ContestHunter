using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace ContestHunter.Models.View.User
{
    public class LoginModel
    {
        [Required]
        [Display(Name="用户名")]
        [MaxLength(50)]
        public string UserName { get; set; }

        [Required]
        [Display(Name="密码")]
        public string Password { get; set; }
    }
}