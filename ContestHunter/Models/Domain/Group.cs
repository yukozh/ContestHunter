using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Database;
namespace ContestHunter.Models.Domain
{
    public class Group
    {
        static void CheckPriviledge()
        {
            if (!User.CurrentUser.groups.Contains("Administrators"))
                throw new PermissionDeniedException();
        }

        /// <summary>
        /// 获得所有用户组
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PermissionDeniedException"></exception>
        public static string[] All()
        {
            CheckPriviledge();
            using(var db = new CHDB())
            {
                return  (from g in db.GROUPs
                                 select g.Name).ToArray();
            }
        }

        /// <summary>
        /// 获得指定用户名所属所有用户组
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="PermissionDeniedException"></exception>
        public static string[] ByUsername(string name)
        {
            CheckPriviledge();
            using (var db = new CHDB())
            {
                return (from u in db.USERs
                       where u.Name==name
                       select u.GROUPs.Select(g=>g.Name)).Single().ToArray();
            }
        }

        /// <summary>
        /// 获得指定用户组的所有用户
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="PermissionDeniedException"></exception>
        public static string[] Users(string name)
        {
            CheckPriviledge();
            using (var db = new CHDB())
            {
                return (from g in db.GROUPs
                        where g.Name == name
                        select g.USERs.Select(u => u.Name)).Single().ToArray();
            }
        }

        /// <summary>
        /// 增加用户组
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="PermissionDeniedException"></exception>
        public static void Add(string name)
        {
            CheckPriviledge();
            using (var db = new CHDB())
            {
                db.GROUPs.Add(new GROUP()
                {
                    ID = Guid.NewGuid(),
                    Name = name
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 移除用户组
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="PermissionDeniedException"></exception>
        public static void Remove(string name)
        {
            CheckPriviledge();
            using (var db = new CHDB())
            {
                db.GROUPs.Remove(
                    (from g in db.GROUPs
                     where g.Name == name
                     select g).Single()
                    );
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 为指定用户组增加用户
        /// </summary>
        /// <param name="grp"></param>
        /// <param name="user"></param>
        /// <exception cref="PermissionDeniedException"></exception>
        public static void AddUser(string grp, string user)
        {
            CheckPriviledge();
            using (var db = new CHDB())
            {
                (from u in db.USERs
                 where u.Name==user
                 select u).Single().GROUPs.Add
                 (
                    (from g in db.GROUPs
                    where g.Name==grp
                    select g).Single()
                    );
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 将用户从指定用户组删除
        /// </summary>
        /// <param name="grp"></param>
        /// <param name="user"></param>
        /// <exception cref="PermissionDeniedException"></exception>
        public static void RemoveUser(string grp, string user)
        {
            CheckPriviledge();
            using (var db = new CHDB())
            {
                (from u in db.USERs
                 where u.Name == user
                 select u).Single().GROUPs.Remove
                 (
                    (from g in db.GROUPs
                     where g.Name == grp
                     select g).Single()
                     );
                db.SaveChanges();
            }
        }


    } 
}