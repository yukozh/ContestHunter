using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
namespace AllKorrect
{
    public class NativeRunner : IDisposable
    {
        TcpClient tcp;
        BinaryReader reader;
        BinaryWriter writer;

        public NativeRunner(string host, int port)
        {
            tcp = new TcpClient(host, port);
            reader = new BinaryReader(tcp.GetStream(), Encoding.ASCII, true);
            writer = new BinaryWriter(tcp.GetStream(), Encoding.ASCII, true);
        }

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

                Message reply=new Message(reader);
                if (reply.Type != MessageType.HasBlobReply)
                {
                    throw new Exception("错误的的消息回应类型");
                }
                return BitConverter.ToInt32(reply.Body, 0) == 1;
            }
        }

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

        public ExecuteResult Execute(string command, IEnumerable<string> argv, long memoryLimit, int timeLimit, long outputLimit, RestrictionLevel restriction, string inputBlob)
        {
            new Exec()
            {
                ArgumentCount = argv.Count(),
                Arguments = argv,
                Command = command,
                InputBlob = inputBlob,
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
            tcp.Close();
        }
    }
}
