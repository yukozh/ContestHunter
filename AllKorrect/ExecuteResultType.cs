using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllKorrect
{
    public enum ExecuteResultType
    {
        /// <summary>
        /// 程序正常结束，且返回值为0
        /// </summary>
        Success,
        Failure,
        Crashed,
        TimeLimitExceeded,
        MemoryLimitExceeded,
        OutputLimitExceeded,
        Violation,
        MathError,
        MemoryAccessViolation,
    }
}
