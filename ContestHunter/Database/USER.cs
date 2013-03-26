//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace ContestHunter.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class USER
    {
        public USER()
        {
            this.CHAT_COMMON = new HashSet<CHAT_COMMON>();
            this.CONTEST_ATTEND = new HashSet<CONTEST_ATTEND>();
            this.HUNTs = new HashSet<HUNT>();
            this.PROBLEMs = new HashSet<PROBLEM>();
            this.RATINGs = new HashSet<RATING>();
            this.RECORDs = new HashSet<RECORD>();
            this.CONTESTs = new HashSet<CONTEST>();
            this.LOCKs = new HashSet<PROBLEM>();
            this.GROUPs = new HashSet<GROUP>();
        }
    
        public System.Guid ID { get; set; }
        public string Name { get; set; }
        public byte[] Password { get; set; }
        public string Email { get; set; }
        public Nullable<int> Rating { get; set; }
        public Nullable<int> PreferLanguage { get; set; }
        public string RealName { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string School { get; set; }
        public Nullable<System.DateTime> LastLoginTime { get; set; }
        public string LastLoginIP { get; set; }
        public string Motto { get; set; }
        public bool AcceptEmail { get; set; }
    
        public virtual ICollection<CHAT_COMMON> CHAT_COMMON { get; set; }
        public virtual ICollection<CONTEST_ATTEND> CONTEST_ATTEND { get; set; }
        public virtual ICollection<HUNT> HUNTs { get; set; }
        public virtual ICollection<PROBLEM> PROBLEMs { get; set; }
        public virtual ICollection<RATING> RATINGs { get; set; }
        public virtual ICollection<RECORD> RECORDs { get; set; }
        public virtual ICollection<CONTEST> CONTESTs { get; set; }
        public virtual ICollection<PROBLEM> LOCKs { get; set; }
        public virtual ICollection<GROUP> GROUPs { get; set; }
    }
}
