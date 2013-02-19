using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ContestHunter.Models.Domain;
namespace ContestHunter.Models.View
{
    public class ContestBasicInfoModel
    {
        [Required]
        [Display(Name = "比赛名称")]
        [MaxLength(50)]
        public string Name { get; set; }

        [Display(Name = "Official", Description = "如果比赛为Official，其成绩会记入能力排名。<br/>能力等级在猎手(Hunter,2100)以下的用户，通过<b>自助</b>操作，只能申办Unofficial的比赛。<br/>如果您想申办一场Official的比赛，请在完成比赛配置和题目添加后尽快联系管理员进行审核。")]
        public bool IsOfficial { get; set; }

        [Display(Name = "比赛管理者", Description = "最多三位比赛管理者，必须包含自己，少于三位请留空其中1~2个文本框")]
        public string Owner1 { get; set; }
        public string Owner2 { get; set; }
        public string Owner3 { get; set; }

        [Required]
        [Display(Name = "赛制")]
        public Contest.ContestType? Type { get; set; }

        [Required]
        [Display(Name = "开始时刻", Description = "比赛将于何时开始")]
        public DateTime? StartTime { get; set; }

        [Required]
        [Display(Name = "小时")]
        [Range(0, int.MaxValue)]
        public int? Hour { get; set; }

        [Required]
        [Display(Name = "分钟")]
        [Range(0, 59)]
        public int? Minute { get; set; }
    }
}