using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
namespace AllKorrect
{
    /// <summary>
    /// 封装好的NativeRunner类。此类为IDisposable，注意适当处理
    /// </summary>
    public sealed class NativeRunner : IDisposable
    {
        const int RANDOM_STRING_LENGTH = 10;
        static readonly Random RAND = new Random();

        TcpClient tcp;
        BinaryReader reader;
        BinaryWriter writer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host">AllKorrect的主机地址</param>
        /// <param name="port">AllKorrect的端口号</param>
        public NativeRunner(string host, int port)
        {
            tcp = new TcpClient(host, port);
            tcp.SendTimeout = 5000;
            reader = new BinaryReader(tcp.GetStream(), Encoding.ASCII, true);
            writer = new BinaryWriter(tcp.GetStream(), Encoding.ASCII, true);
        }

        /// <summary>
        /// 将特定Blob移动为特定的File
        /// </summary>
        public void MoveBlob2File(string blob, string file)
        {
            new CopyMove
            {
                MsgType = MessageType.MoveBlob2File,
                OldName = blob,
                NewName = file
            }.ToMessage().Send(writer);
            if (new Message(reader).Type != MessageType.OK)
            {
                throw new Exception("移动或复制失败");
            }
        }

        /// <summary>
        /// 为特定Blob更名
        /// </summary>
        public void MoveBlob2Blob(string blob1, string blob2)
        {
            new CopyMove
            {
                MsgType = MessageType.MoveBlob2Blob,
                OldName = blob1,
                NewName = blob2
            }.ToMessage().Send(writer);
            if (new Message(reader).Type != MessageType.OK)
            {
                throw new Exception("移动或复制失败");
            }
        }

        /// <summary>
        /// 为特定File更名
        /// </summary>
        public void MoveFile2File(string file1, string file2)
        {
            new CopyMove
            {
                MsgType = MessageType.MoveFile2File,
                OldName = file1,
                NewName = file2
            }.ToMessage().Send(writer);
            if (new Message(reader).Type != MessageType.OK)
            {
                throw new Exception("移动或复制失败");
            }
        }

        /// <summary>
        /// 将特定File移动为特定的Blob
        /// </summary>
        public void MoveFile2Blob(string file, string blob)
        {
            new CopyMove
            {
                MsgType = MessageType.MoveFile2Blob,
                OldName = file,
                NewName = blob
            }.ToMessage().Send(writer);
            if (new Message(reader).Type != MessageType.OK)
            {
                throw new Exception("移动或复制失败");
            }
        }

        /// <summary>
        /// 复制特定Blob为特定的File
        /// </summary>
        public void CopyBlob2File(string blob, string file)
        {
            new CopyMove
            {
                MsgType = MessageType.CopyBlob2File,
                OldName = blob,
                NewName = file
            }.ToMessage().Send(writer);
            if (new Message(reader).Type != MessageType.OK)
            {
                throw new Exception("移动或复制失败");
            }
        }

        /// <summary>
        /// 复制特定Blob为特定的Blob
        /// </summary>
        public void CopyBlob2Blob(string blob1, string blob2)
        {
            new CopyMove
            {
                MsgType = MessageType.CopyBlob2Blob,
                OldName = blob1,
                NewName = blob2
            }.ToMessage().Send(writer);
            if (new Message(reader).Type != MessageType.OK)
            {
                throw new Exception("移动或复制失败");
            }
        }

        /// <summary>
        /// 复制特定File为特定的File
        /// </summary>
        public void CopyFile2File(string file1, string file2)
        {
            new CopyMove
            {
                MsgType = MessageType.CopyFile2File,
                OldName = file1,
                NewName = file2
            }.ToMessage().Send(writer);
            if (new Message(reader).Type != MessageType.OK)
            {
                throw new Exception("移动或复制失败");
            }
        }

        /// <summary>
        /// 复制特定File为特定的Blob
        /// </summary>
        public void CopyFile2Blob(string file, string blob)
        {
            new CopyMove
            {
                MsgType = MessageType.CopyFile2Blob,
                OldName = file,
                NewName = blob
            }.ToMessage().Send(writer);
            if (new Message(reader).Type != MessageType.OK)
            {
                throw new Exception("移动或复制失败");
            }
        }

        /// <summary>
        /// 判断某Blob是否存在
        /// </summary>
        public bool HasBlob(string name)
        {
            using (MemoryStream mem = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(mem))
                {
                    bw.Write(name);
                }
                Message msg = new Message
                {
                    Type = MessageType.HasBlob,
                    Body = mem.ToArray()
                };
                msg.Size = msg.Body.Length;
                msg.Send(writer);

                Message reply = new Message(reader);
                if (reply.Type != MessageType.HasBlobReply)
                {
                    throw new Exception("错误的的消息回应类型");
                }
                return BitConverter.ToInt32(reply.Body, 0) == 1;
            }
        }

        /// <summary>
        /// 判断某File是否存在
        /// </summary>
        public bool HasFile(string name)
        {
            using (MemoryStream mem = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(mem))
                {
                    bw.Write(name);
                }
                Message msg = new Message
                {
                    Type = MessageType.HasFile,
                    Body = mem.ToArray()
                };
                msg.Size = msg.Body.Length;
                msg.Send(writer);

                Message reply = new Message(reader);
                if (reply.Type != MessageType.HasFileReply)
                {
                    throw new Exception("错误的的消息回应类型");
                }
                return BitConverter.ToInt32(reply.Body, 0) == 1;
            }
        }

        /// <summary>
        /// 上传一个Blob
        /// </summary>
        public void PutBlob(string name, byte[] bytes)
        {
            new PutBlob()
            {
                Name = name,
                Size = bytes.Length,
                Bytes = bytes
            }.ToMessage().Send(writer);
            if (new Message(reader).Type != MessageType.OK)
            {
                throw new Exception("添加Blob失败");
            }
        }

        /// <summary>
        /// 下载一个Blob
        /// </summary>
        public byte[] GetBlob(string name)
        {
            new GetBlob()
            {
                Name = name
            }.ToMessage().Send(writer);
            var reply = new Message(reader);
            if (reply.Type != MessageType.GetBlobReply)
            {
                throw new Exception("收到错误的消息类型");
            }
            return reply.Body;
        }

        /// <summary>
        /// 下载一个Blob的部分字节
        /// </summary>
        /// <param name="skip">跳过前多少字节</param>
        /// <param name="top">继续读取最多多少字节</param>
        public byte[] GetBlob(string name, int skip, int top)
        {
            new GetBlobParticle()
            {
                Name = name,
                Skip = skip,
                Top = top
            }.ToMessage().Send(writer);
            var reply = new Message(reader);
            if (reply.Type != MessageType.GetBlobParticleReply)
            {
                throw new Exception("收到错误的消息类型");
            }
            return reply.Body;
        }

        /// <summary>
        /// 获取Blob的字节长度
        /// </summary>
        public int GetBlobLength(string name)
        {
            new GetBlobLength()
            {
                Name = name
            }.ToMessage().Send(writer);
            var reply = new Message(reader);
            if (reply.Type != MessageType.GetBlobLengthReply)
            {
                throw new Exception("收到错误的消息类型");
            }
            return BitConverter.ToInt32(reply.Body, 0);
        }

        /// <summary>
        /// 获取File的字节长度
        /// </summary>
        public int GetFileLength(string name)
        {
            string tmpBlob = RandomString();
            MoveFile2Blob(name, tmpBlob);
            int result = GetBlobLength(tmpBlob);
            MoveBlob2File(tmpBlob, name);
            return result;
        }

        /// <summary>
        /// 上传一个File
        /// </summary>
        public void PutFile(string name, byte[] bytes)
        {
            string filename = RandomString();
            PutBlob(filename, bytes);
            MoveBlob2File(filename, name);
        }

        /// <summary>
        /// 下载一个File
        /// </summary>
        public byte[] GetFile(string name)
        {
            string blobname = RandomString();
            MoveFile2Blob(name, blobname);
            byte[] result = GetBlob(blobname);
            MoveBlob2File(blobname, name);
            return result;
        }

        /// <summary>
        /// 下载一个File的部分字节
        /// </summary>
        /// <param name="skip">跳过前多少字节</param>
        /// <param name="top">继续读取最多多少字节</param>
        public byte[] GetFile(string name, int skip, int top)
        {
            string tmpBlob = RandomString();
            MoveFile2Blob(name, tmpBlob);
            byte[] result = GetBlob(tmpBlob, skip, top);
            MoveBlob2File(tmpBlob, name);
            return result;
        }

        /// <summary>
        /// 远程执行一个程序
        /// </summary>
        /// <param name="command">若为系统程序，直接填入程序名，如g++，若为某File，则在程序名前加"./"，如./code</param>
        /// <param name="argv">程序的命令行参数，如new[]{"-o","code","code.cpp"}</param>
        /// <param name="memoryLimit">运行时内存限制，单位为字节。-1为不限制。</param>
        /// <param name="timeLimit">运行时用户态时间限制，单位为毫秒。-1为不限制。</param>
        /// <param name="outputLimit">程序输出大小限制，单位为字节。-1为不限制。</param>
        /// <param name="restriction">程序限制级别</param>
        /// <param name="inputBlob">输入文件的Blob名称，null为没有输出文件。</param>
        /// <returns>程序运行结果</returns>
        public ExecuteResult Execute(string command, IEnumerable<string> argv, long memoryLimit, int timeLimit, long outputLimit, RestrictionLevel restriction, string inputBlob)
        {
            new Exec()
            {
                ArgumentCount = argv.Count(),
                Arguments = argv,
                Command = command,
                InputBlob = inputBlob ?? "",
                MemoryLimit = memoryLimit,
                OutputLimit = outputLimit,
                Restriction = restriction,
                TimeLimit = timeLimit
            }.ToMessage().Send(writer);
            var reply = new Message(reader);
            if (reply.Type != MessageType.ExecuteReply)
            {
                throw new Exception("收到错误的消息类型");
            }
            return new ExecuteResult(reply);
        }

        public void Dispose()
        {
            if (tcp.Connected && writer.BaseStream.CanWrite)
            {
                new Message()
                {
                    Type = MessageType.Exit,
                    Body = new byte[0],
                    Size = 0
                }.Send(writer);
                //Increase the probability of server  to recv the msg
                Thread.Sleep(500);
            }
            tcp.Close();
        }

        /// <summary>
        /// 制造一个随机字符串，可作为Blob或File的名称
        /// </summary>
        public string RandomString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("__");
            for (int i = 0; i < RANDOM_STRING_LENGTH; i++)
            {
                int num = RAND.Next(10 + 26 + 26);
                if (num < 10)
                {
                    sb.Append((char)(num + '0'));
                }
                else if (num < 10 + 26)
                {
                    sb.Append((char)(num - 10 + 'a'));
                }
                else
                {
                    sb.Append((char)(num - 10 - 26 + 'A'));
                }
            }
            return sb.ToString();
        }
    }
}
