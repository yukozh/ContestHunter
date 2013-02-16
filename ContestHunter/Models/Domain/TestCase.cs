using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Models.Domain;
using ContestHunter.Database;

namespace ContestHunter.Models.Domain
{
    public class TestCase
    {
        public Guid ID;
        public byte[] Input;
        public byte[] Data;
        public int TimeLimit;
        public long MemoryLimit;

        /// <summary>
        /// 修改测试数据时间/内存限制
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="newTimeLimit"></param>
        /// <param name="newMemoryLimit"></param>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="PermissionDeniedException"></exception>
        static public void Change(Guid ID, int newTimeLimit,long newMemoryLimit)
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            using (var db = new CHDB())
            {
                var test = (from t in db.TESTDATAs
                            where t.ID == ID
                            select t).Single();
                if (!User.CurrentUser.groups.Contains("Administrators") && !(from u in test.PROBLEM1.CONTEST1.OWNERs
                                                                             select u.Name).Contains(User.CurrentUser.name))
                    throw new PermissionDeniedException();
                test.MemoryLimit = newMemoryLimit;
                test.TimeLimit = newTimeLimit;
                db.SaveChanges();
            }
        }
    }
}