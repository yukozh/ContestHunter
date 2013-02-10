using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ContestHunter.Database;
namespace ContestHunter.Models.Domain
{
    public class Group
    {
        public string Name;

        internal static void CheckPriviledge()
        {
            if (!User.CurrentUser.groups.Contains("Administrators"))
                throw new PermissionDeniedException();
        }

        /// <summary>
        /// 获得所有用户组
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PermissionDeniedException"></exception>
        public static List<Group> All()
        {
            CheckPriviledge();
            using(var db = new CHDB())
            {
                return (from g in db.GROUPs
                        select new Group
                        {
                            Name = g.Name
                        }).ToList();
            }
        }

        /// <summary>
        /// 获得指定用户组的用户
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="PermissionDeniedException"></exception>
        public List<User> Users(int skip,int top)
        {
            CheckPriviledge();
            using (var db = new CHDB())
            {
                return (from u in
                            (from g in db.GROUPs
                             where g.Name == Name
                             select g.USERs).Single()
                        select new User
                        {
                            Name = u.Name,
                            Email = u.Email
                        }).OrderBy(u => u.Name).Skip(skip).Take(top).ToList();
            }
        }

        /// <summary>
        /// 返回用户数量
        /// </summary>
        /// <returns></returns>
        public int UserCount()
        {
            CheckPriviledge();
            using (var db = new CHDB())
            {
                return (from g in db.GROUPs
                        where g.Name == Name
                        select g.USERs).Single().Count();
            }
        }

        /// <summary>
        /// 增加用户组
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="PermissionDeniedException"></exception>
        public static void Add(Group group)
        {
            CheckPriviledge();
            using (var db = new CHDB())
            {
                db.GROUPs.Add(new GROUP()
                {
                    ID = Guid.NewGuid(),
                    Name =group.Name
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 移除用户组
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="PermissionDeniedException"></exception>
        public void Remove()
        {
            CheckPriviledge();
            using (var db = new CHDB())
            {
                db.GROUPs.Remove(
                    (from g in db.GROUPs
                     where g.Name == Name
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
        public void AddUser(User user)
        {
            CheckPriviledge();
            using (var db = new CHDB())
            {
                (from u in db.USERs
                 where u.Name==user.Name
                 select u).Single().GROUPs.Add
                 (
                    (from g in db.GROUPs
                    where g.Name==Name
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
        public void RemoveUser(User user)
        {
            CheckPriviledge();
            using (var db = new CHDB())
            {
                (from u in db.USERs
                 where u.Name == user.Name
                 select u).Single().GROUPs.Remove
                 (
                    (from g in db.GROUPs
                     where g.Name == Name
                     select g).Single()
                     );
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 返回指定名称的用户组
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="PermissionDeniedException"></exception>
        /// <exception cref="GroupNotFoundException"></exception>
        public static Group ByName(string name)
        {
            CheckPriviledge();
            using (var db = new CHDB())
            {
                var result = (from g in db.GROUPs
                              where g.Name == name
                              select new Group
                              {
                                  Name = g.Name
                              }).SingleOrDefault();
                if (null == result)
                    throw new GroupNotFoundException();

                return result;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Group)
                return Name == ((Group)obj).Name;
            return base.Equals(obj);
        }
    } 
}