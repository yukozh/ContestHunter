using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace AllKorrect
{
    /// <summary>
    /// Execute的执行结果
    /// </summary>
    public class ExecuteResult
    {
        /// <summary>
        /// 程序的返回值，当且仅当Type为Success或Failure时有意义
        /// </summary>
        public int ExitStatus;
        /// <summary>
        /// 程序运行结果类型
        /// </summary>
        public ExecuteResultType Type;
        /// <summary>
        /// 标准输出的Blob名称
        /// </summary>
        public string OutputBlob;
        /// <summary>
        /// 标准错误的Blob名称
        /// </summary>
        public string ErrorBlob;
        /// <summary>
        /// 程序的执行时，占用的最大内存，以字节为单位
        /// </summary>
        public long Memory;
        /// <summary>
        /// 程序执行的用户态时间，以毫秒为单位
        /// </summary>
        public int Time;

        internal ExecuteResult(Message msg)
        {
            using (MemoryStream mem = new MemoryStream(msg.Body))
            {
                using (BinaryReader reader = new BinaryReader(mem))
                {
                    ExitStatus = reader.ReadInt32();
                    Type = (ExecuteResultType)reader.ReadInt32();
                    OutputBlob = reader.ReadString();
                    ErrorBlob = reader.ReadString();
                    Memory = reader.ReadInt64();
                    Time = reader.ReadInt32();
                }
            }
        }
    }
}
