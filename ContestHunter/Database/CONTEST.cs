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
    
    public partial class CONTEST
    {
        public CONTEST()
        {
            this.PROBLEMs = new HashSet<PROBLEM>();
            this.USERs = new HashSet<USER>();
            this.USERs1 = new HashSet<USER>();
        }
    
        public System.Guid ID { get; set; }
        public string Name { get; set; }
        public System.DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
    
        public virtual ICollection<PROBLEM> PROBLEMs { get; set; }
        public virtual ICollection<USER> USERs { get; set; }
        public virtual ICollection<USER> USERs1 { get; set; }
    }
}
