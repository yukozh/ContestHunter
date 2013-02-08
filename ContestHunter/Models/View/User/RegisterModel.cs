using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ContestHunter.Models.View.User
{
    public class RegisterModel
    {
        [Required]
        [Display(Name = "用户名", Description = "您的账户名称，50字以内")]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码", Description = "找一个安全的密码，并牢记它")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "电子邮件", Description = "合法的Email地址，它将用于接收验证邮件与密码恢复")]
        [RegularExpression(@".+@.+\..{2,4}",ErrorMessage="无效的邮件地址")]
        [MaxLength(100)]
        public string Email { get; set; }
    }
}