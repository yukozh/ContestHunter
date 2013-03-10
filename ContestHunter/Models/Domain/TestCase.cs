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
        internal byte[] _Input;
        internal byte[] _Data;
        bool inputFromSetter;
        bool dataFromSetter;
        public byte[] Input
        {
            get
            {
                if (inputFromSetter) return _Input;
                if (!canGetData)
                    return null;
                using (var db = new CHDB())
                {
                    return (from t in db.TESTDATAs
                            where t.ID == ID
                            select t.Input).FirstOrDefault();
                }
            }
            set
            {
                inputFromSetter = true;
                _Input = value;
            }
        }
        public byte[] Data
        {
            get
            {
                if (dataFromSetter) return _Data;
                if (!canGetData)
                    return null;
                using (var db = new CHDB())
                {
                    return (from t in db.TESTDATAs
                            where t.ID == ID
                            select t.Data).FirstOrDefault();
                }
            }
            set
            {
                dataFromSetter = true;
                _Data = value;
            }
        }
        public int TimeLimit;
        public long MemoryLimit;
        public bool Available;
        internal bool canGetData;

        /// <summary>
        /// 修改测试数据时间/内存限制
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="newTimeLimit"></param>
        /// <param name="newMemoryLimit"></param>
        /// <exception cref="UserNotLoginException"></exception>
        /// <exception cref="PermissionDeniedException"></exception>
        static public void Change(Guid ID, int newTimeLimit,long newMemoryLimit,bool newAvailable)
        {
            if (null == User.CurrentUser)
                throw new UserNotLoginException();
            using (var db = new CHDB())
            {
                var test = (from t in db.TESTDATAs
                            where t.ID == ID
                            select t).Single();
                if (!User.CurrentUser.IsAdmin && !(from u in test.PROBLEM1.CONTEST1.OWNERs
                                                                             select u.Name).Contains(User.CurrentUser.name))
                    throw new PermissionDeniedException();
                test.MemoryLimit = newMemoryLimit;
                test.TimeLimit = newTimeLimit;
                test.Available = newAvailable;
                db.SaveChanges();
            }
        }
    }
}