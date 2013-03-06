using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace ContestHunter.Models.View
{
    public class UserEditModel
    {
        public string Email { get; set; }

        [Required]
        [Display(Name="当前密码",Description="请输入您现在的密码")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }
        
        [Display(Name="新密码",Description="若不修改密码，请留空")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "真实姓名", Description = "鼓励您填写真实信息，以便于大家相互交流")]
        public string RealName { get; set; }

        [Display(Name="国家")]
        public string Country { get; set; }

        [Display(Name="城市")]
        public string City { get; set; }

        [Display(Name="省份")]
        public string Province { get; set; }

        [Display(Name="学校")]
        public string School { get; set; }

        [Display(Name="我的格言",Description="对自己的简要介绍，或者您想说的话")]
        public string Motto { get; set; }

        [Display(Name = "接收系统邮件", Description = "接收ContestHunter发送的比赛预告等邮件。")]
        public bool ReceiveEmail { get; set; }
    }
}