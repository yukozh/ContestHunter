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
            this.RECORDs = new HashSet<RECORD>();
            this.CONTESTs = new HashSet<CONTEST>();
            this.CONTESTs1 = new HashSet<CONTEST>();
            this.GROUPs = new HashSet<GROUP>();
        }
    
        public System.Guid ID { get; set; }
        public string Name { get; set; }
        public byte[] Password { get; set; }
        public string Email { get; set; }
    
        public virtual ICollection<RECORD> RECORDs { get; set; }
        public virtual ICollection<CONTEST> CONTESTs { get; set; }
        public virtual ICollection<CONTEST> CONTESTs1 { get; set; }
        public virtual ICollection<GROUP> GROUPs { get; set; }
    }
}
