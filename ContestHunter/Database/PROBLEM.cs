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
    
    public partial class PROBLEM
    {
        public PROBLEM()
        {
            this.RECORDs = new HashSet<RECORD>();
            this.TESTDATAs = new HashSet<TESTDATA>();
        }
    
        public System.Guid ID { get; set; }
        public System.Guid Contest { get; set; }
        public string Name { get; set; }
        public byte[] Content { get; set; }
        public string Comparer { get; set; }
    
        public virtual CONTEST CONTEST1 { get; set; }
        public virtual ICollection<RECORD> RECORDs { get; set; }
        public virtual ICollection<TESTDATA> TESTDATAs { get; set; }
    }
}