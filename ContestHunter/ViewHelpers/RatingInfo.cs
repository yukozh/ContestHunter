using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Models.Domain;
namespace ContestHunter.ViewHelpers
{
    public class RatingInfo
    {
        public string Color, Caption;
        public int Rating;

        public RatingInfo(string userName)
            : this(User.ByName(userName))
        {
        }

        public RatingInfo(int rating)
        {
            Rating = rating;
            if (rating >= 2600 && rating <= 3000)
            {
                Caption = "国际首席猎手(International Chief Hunter)";
                Color = "#FF0000";
            }
            else if (rating >= 2350)
            {
                Caption = "首席猎手(Chief Hunter)";
                Color = "#F66F00";
            }
            else if (rating >= 2100)
            {
                Caption = "猎手(Hunter)";
                Color = "#FF8C00";
            }
            else if (rating >= 1900)
            {
                Caption = "神犇(God Triple Bull)";
                Color = "#CC0066";
            }
            else if (rating >= 1700)
            {
                Caption = "神牛(God Bull)";
                Color = "#3366CC";
            }
            else if (rating >= 1500)
            {
                Caption = "大牛(Bull)";
                Color = "#0099CC";
            }
            else if (rating >= 1250)
            {
                Caption = "领先学习者(Advanced Learner)";
                Color = "#008000";
            }
            else if (rating >= 1000)
            {
                Caption = "学习者(Learner)";
                Color = "#77CC77";
            }
            else if (rating >= 1)
            {
                Caption = "蒟蒻(Weak Dish)";
                Color = "#C0C0C0";
            }
            else
            {
                Caption = "尚未排名的(Unrated)";
                Color = "#CC9966";
            }
        }

        public RatingInfo(User user)
            : this(user.Rating)
        {
            if (user.IsAdmin)
            {
                Caption = "管理员(Administrator)";
                Color = "#000000";
            }
        }
    }
}