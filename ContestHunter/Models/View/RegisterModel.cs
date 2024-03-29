﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ContestHunter.Models.View
{
    public class RegisterModel
    {
        [Required]
        [Display(Name = "用户名", Description = "您的账户名称，20字以内，请勿使用 . / % : # 等特殊字符")]
        [MaxLength(20)]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码", Description = "找一个安全的密码，并牢记它")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "电子邮件", Description = "合法的Email地址，它将用于接收验证邮件与密码恢复")]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
    }
}