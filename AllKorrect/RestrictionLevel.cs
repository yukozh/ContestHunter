using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllKorrect
{
    /// <summary>
    /// 程序运行时限制级别
    /// </summary>
    public enum RestrictionLevel
    {
        /// <summary>
        /// 严格，限制较多
        /// </summary>
        Strict,
        /// <summary>
        /// 宽松，限制较少
        /// </summary>
        Loose
    }
}
