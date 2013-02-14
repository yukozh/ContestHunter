using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net.Sockets;
using System.Net;
using ContestHunter.Database;
using System.Text;
using System.Configuration;

namespace ContestHunter.Models.Domain
{
    public class TesterDaemon : Daemon
    {
        static IDictionary<int, IDictionary<string, string>> commands = new Dictionary<int, IDictionary<string, string>>()
        {
            {
                (int)Record.LanguageType.CPP,
                new Dictionary<string,string>()
                {
                    {"src2exe","g++ -Wall -O2 -o {Execute} {E.exeE} {Source} {S.cppS}"},
                    {"src2obj","g++ -Wall -O2 -c -o {Execute} {E.oE} {Source} {S.cppS}"},
                    {"obj2exe","g++ -Wall -O2 -o {Execute} {E.exeE} {Object}"},
                    {"execute","{Execute}"}
                }
            },
            {
                (int)Record.LanguageType.C,
                new Dictionary<string,string>()
                {
                    {"src2exe","gcc -Wall -O2 -o {Execute} {E.exeE} {Source} {S.cS}"},
                    {"src2obj","gcc -Wall -O2 -c -o {Execute} {E.oE} {Source} {S.cS}"},
                    {"obj2exe","gcc -Wall -O2 -o {Execute} {E.exeE} {Object}"},
                    {"execute","{Execute}"}
                }
            },
            {
                (int)Record.LanguageType.Pascal,
                new Dictionary<string,string>()
                {
                    {"src2exe","ppcrossx64 -o{Execute} {E.exeE} {Source} {S.pasS}"},
                    {"execute","{Execute}"}
                }
            },
            {
                (int)Record.LanguageType.Java,
                new Dictionary<string,string>()
                {
                    {"src2exe","javac {Source=Main} {S.javaS}"},
                    {"execute","java Main"}
                }
            }
        };

        public string IP = "moo.imeng.de";
        public long Password = 34659308463532339;
        protected override int Run()
        {
            using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                sock.Connect(new IPEndPoint(IPAddress.Parse(IP), 6000));
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter Writer = new BinaryWriter(stream))
                    {
                        //Password Here!!
                        Writer.Write(Password);
                    }
                    sock.Send(stream.ToArray());
                }
                using (var db = new CHDB())
                {
                    var rec = (from r in db.RECORDs
                               where r.Status == (int)Record.StatusType.Pending
                               select r).FirstOrDefault();
                    if (null == rec)
                        return 3000;
                    sock.Send(new Message()
                    {
                        Type = Message.MessageType.Compile,
                        Content = new CompileIn()
                        {
                            Code = rec.Code,
                            Command = commands[rec.Language]["src2exe"],
                            Memory = 1024 * 1024 * 60,
                            Time = 5 * 1000
                        }
                    }.ToBytes());
                    Out ret = new Out(sock);
                    switch (ret.Type)
                    {
                        case Out.ResultType.Success:
                            sock.Send(new Message()
                            {
                                Type = Message.MessageType.Compile,
                                Content = new CompileIn()
                                {
                                    Code = rec.Code,
                                    Command = commands[rec.Language]["src2exe"],
                                    Memory = 1024 * 1024 * 60,
                                    Time = 5 * 1000
                                }
                            }.ToBytes());
                            Out CompileCMP = new Out(sock);
                            switch (CompileCMP.Type)
                            {
                                case Out.ResultType.Success:
                                    int totalTests = 0;
                                    int passedTests = 0;
                                    foreach (TESTDATA test in (from t in db.TESTDATAs
                                                               where t.PROBLEM1 == rec.PROBLEM1
                                                               select t))
                                    {
                                        totalTests++;
                                        sock.Send(new Message()
                                        {
                                            Type = Message.MessageType.Test,
                                            Content = new TestIn()
                                            {
                                                CmpPath = CompileCMP.Message,
                                                ExecPath = ret.Message,
                                                Memory = test.MemoryLimit,
                                                Time = test.TimeLimit,
                                                Input = test.Input,
                                                Output = test.Data
                                            }
                                        }.ToBytes());
                                        Out testResult = new Out(sock);
                                        switch (testResult.Type)
                                        {
                                            case Out.ResultType.Success:
                                                passedTests++;
                                                rec.MemoryUsed += testResult.Memory;
                                                rec.ExecutedTime += (int)testResult.Time;
                                                break;
                                            default:
                                                rec.Status = (int)testResult.Type;
                                                break;
                                        }
                                    }
                                    if (totalTests == passedTests)
                                    {
                                        rec.Status = (int)Out.ResultType.Success;
                                    }
                                    rec.Score = (0 != totalTests ? passedTests / totalTests * 100 : 0);
                                    break;
                                default:
                                    rec.Status = (int)Record.StatusType.CMP_Error;
                                    break;
                            }
                            break;
                        default:
                            rec.Status = (int)Record.StatusType.Compile_Error;
                            break;
                    }
                    db.SaveChanges();
                }
            }
            return 3000;
        }

        public class Out
        {
            public enum ResultType : uint
            {
                Success, WrongAnswer, TimeLimitExceeded, RuntimeError, MemoryLimitExceeded, CompareError, OutputLimitExceeded
            }

            public ResultType Type { get; set; }
            public long Time { get; set; }
            public long Memory { get; set; }
            public string Message;

            void ReadIntoBuffer(Socket sock, byte[] buf)
            {
                int haveRead = 0;
                while (haveRead < buf.Length)
                {
                    int currentRead = sock.Receive(buf, haveRead, buf.Length - haveRead, SocketFlags.None);
                    if (currentRead == 0)
                    {
                        throw new Exception("ReadIntoBuffer No Enough Bytes!");
                    }
                    haveRead += currentRead;
                }
            }

            public Out(Socket sock)
            {
                byte[] buf = new byte[sizeof(uint) + sizeof(uint) + sizeof(long) + sizeof(long)];
                ReadIntoBuffer(sock, buf);

                uint messageLength;
                using (MemoryStream stream = new MemoryStream(buf))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        messageLength = reader.ReadUInt32();
                        Type = (ResultType)reader.ReadUInt32();
                        Time = reader.ReadInt64();
                        Memory = reader.ReadInt64();
                    }
                }

                if (messageLength > 0)
                {
                    buf = new byte[messageLength];
                    sock.Receive(buf);
                    Message = Encoding.Default.GetString(buf);
                }
                else
                {
                    Message = "";
                }
            }

            public override string ToString()
            {
                return "[Out Type=" + Type + " Time=" + Time + " Memory=" + Memory + " Messsage=" + Message + "]";
            }
        }

        public class Message
        {
            public enum MessageType : uint
            {
                Compile = 1, Test = 2
            }

            public MessageType Type { get; set; }
            public IMessageContent Content { get; set; }

            public byte[] ToBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write((uint)Type);
                        byte[] contentBytes = Content.ToBytes();
                        writer.Write((uint)contentBytes.LongLength);
                        writer.Write(contentBytes);
                    }
                    return stream.ToArray();
                }
            }
        }

        public interface IMessageContent
        {
            byte[] ToBytes();
        }

        public class CompileIn : IMessageContent
        {
            public long Time { get; set; }
            public long Memory { get; set; }
            public string Code { get; set; }
            public string Command { get; set; }

            public byte[] ToBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(Time);
                        writer.Write(Memory);
                        byte[] code = Encoding.Default.GetBytes(Code);
                        writer.Write((uint)code.Length);
                        writer.Write(code);
                        writer.Write(Encoding.Default.GetBytes(Command));
                        writer.Write((byte)0);
                    }
                    return stream.ToArray();
                }
            }
        }

        public class TestIn : IMessageContent
        {
            public long Time { get; set; }
            public long Memory { get; set; }
            public string ExecPath { get; set; }
            public string CmpPath { get; set; }
            public byte[] Input { get; set; }
            public byte[] Output { get; set; }

            public byte[] ToBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        byte[] execPath = Encoding.Default.GetBytes(ExecPath);
                        byte[] cmpPath = Encoding.Default.GetBytes(CmpPath);
                        writer.Write((uint)0);
                        writer.Write((uint)(execPath.Length + 1));
                        writer.Write((uint)(execPath.Length + 1 + Input.Length));
                        writer.Write((uint)(execPath.Length + 1 + Input.Length + Output.Length));
                        writer.Write(Time);
                        writer.Write(Memory);
                        writer.Write(execPath);
                        writer.Write((byte)0);
                        writer.Write(Input);
                        writer.Write(Output);
                        writer.Write(cmpPath);
                        writer.Write((byte)0);
                    }
                    return stream.ToArray();
                }
            }
        }
    }
}