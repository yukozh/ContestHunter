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
    
    public partial class MESSAGE
    {
        public System.Guid ID { get; set; }
        public System.Guid Session { get; set; }
        public System.Guid User { get; set; }
        public System.DateTime Time { get; set; }
        public string Content { get; set; }
    
        public virtual SESSION SESSION1 { get; set; }
        public virtual USER USER1 { get; set; }
    }
}
