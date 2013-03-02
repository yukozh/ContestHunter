using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace ContestHunter.Models.View
{
    public class LoginModel
    {
        [Required]
        [Display(Name = "用户名")]
        [MaxLength(20)]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "保持登录状态", Description = "选中后，即使您的浏览器关闭了，您将仍然保持登录状态")]
        public bool KeepOnline { get; set; }
    }
}