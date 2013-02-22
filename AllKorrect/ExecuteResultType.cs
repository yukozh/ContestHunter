using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllKorrect
{
    /// <summary>
    /// 程序运行结果类型
    /// </summary>
    public enum ExecuteResultType
    {
        /// <summary>
        /// 程序正常结束，且返回值为0
        /// </summary>
        Success,
        /// <summary>
        /// 程序已退出，但返回值不为0
        /// </summary>
        Failure,
        /// <summary>
        /// 程序崩溃
        /// </summary>
        Crashed,
        /// <summary>
        /// 程序运行时间超出限制
        /// </summary>
        TimeLimitExceeded,
        /// <summary>
        /// 程序占用内存超出限制
        /// </summary>
        MemoryLimitExceeded,
        /// <summary>
        /// 程序输出大小超出限制
        /// </summary>
        OutputLimitExceeded,
        /// <summary>
        /// 程序试图执行非法操作，被强行终止
        /// </summary>
        Violation,
        /// <summary>
        /// 程序内部产生数学错误
        /// </summary>
        MathError,
        /// <summary>
        /// 程序访问了非法的内存地址
        /// </summary>
        MemoryAccessViolation,
    }
}
