using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net.Sockets;
using System.Net;
using ContestHunter.Database;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Web.Http;

namespace ContestHunter.Models.Domain
{
    public class TesterDaemon : Daemon
    {
        static IDictionary<Record.LanguageType, IDictionary<string, string>> commands = new Dictionary<Record.LanguageType, IDictionary<string, string>>()
        {
            {
                Record.LanguageType.CPP,
                new Dictionary<string,string>()
                {
                    {"src2exe","g++ -Wall -O2 -o {Execute} {E.exeE} {Source} {S.cppS}"},
                    {"src2obj","g++ -Wall -O2 -c -o {Execute} {E.oE} {Source} {S.cppS}"},
                    {"obj2exe","g++ -Wall -O2 -o {Execute} {E.exeE} {Object}"},
                    {"execute","{Execute}"}
                }
            },
            {
                Record.LanguageType.C,
                new Dictionary<string,string>()
                {
                    {"src2exe","gcc -Wall -O2 -o {Execute} {E.exeE} {Source} {S.cS}"},
                    {"src2obj","gcc -Wall -O2 -c -o {Execute} {E.oE} {Source} {S.cS}"},
                    {"obj2exe","gcc -Wall -O2 -o {Execute} {E.exeE} {Object}"},
                    {"execute","{Execute}"}
                }
            },
            {
                Record.LanguageType.Pascal,
                new Dictionary<string,string>()
                {
                    {"src2exe","ppcrossx64 -o{Execute} {E.exeE} {Source} {S.pasS}"},
                    {"execute","{Execute}"}
                }
            },
            {
                Record.LanguageType.Java,
                new Dictionary<string,string>()
                {
                    {"src2exe","javac {Source=Main} {S.javaS}"},
                    {"execute","java Main"}
                }
            }
        };

        public string IP = "moo.imeng.de";
        public long Password = 34659308463532339;

        Out Compile(string Source, Record.LanguageType Language,Socket sock)
        {
            sock.Send(new Message()
            {
                Type = Message.MessageType.Compile,
                Content = new CompileIn()
                {
                    Code = Source,
                    Command = commands[Language]["src2exe"],
                    Memory = 256 * 1024 * 1024,
                    Time = 5000
                }
            }.ToBytes());
            var Ret = new Out(sock);
            Ret.Message = HttpUtility.HtmlEncode(Regex.Replace(Ret.Message, "\r\n?|\n", "<br/>"));
            return Ret;
        }

        Out Test(byte[] input,byte[] output,long Time,long Memory, Socket sock, string CmpPath, string ExecPath)
        {
            sock.Send(new Message()
            {
                Type = Message.MessageType.Test,
                Content = new TestIn()
                {
                    CmpPath = CmpPath,
                    ExecPath = ExecPath,
                    Memory = Memory,
                    Time = Time,
                    Input = input,
                    Output = output
                }
            }.ToBytes());
            return new Out(sock);
        }

        void DealRecord(CHDB db,Socket sock)
        {

            var rec = (from r in db.RECORDs
                       where r.Status == (int)Record.StatusType.Pending
                       select r).FirstOrDefault();
            if (null == rec)
                return;
            StringBuilder Detail = new StringBuilder();
            Out ret = Compile(rec.Code, (Record.LanguageType)rec.Language, sock);
            if (ret.Type != Out.ResultType.Success)
            {
                rec.Status = (int)Record.StatusType.Compile_Error;
                Detail.AppendFormat("<h5>编译失败：</h5>\r\n<div style=\"padding-left: 10px\">\r\n{0}{1}</div>", ret.Type.ToString(), ret.Message);
                return;
            }
            Detail.Append("<h5>各测试点详细信息：</h5>\r\n<div style=\"padding-left: 10px\">");
            Out CompileCMP = Compile(rec.PROBLEM1.Comparer, (Record.LanguageType)rec.PROBLEM1.ComparerLanguage, sock);
            if (CompileCMP.Type != Out.ResultType.Success)
            {
                rec.Status = (int)Record.StatusType.CMP_Error;
                Detail.Append("比较器编译失败");
                return;
            }
            int totalTests = 0;
            int passedTests = 0;
            foreach (TESTDATA test in (from t in db.TESTDATAs
                                        where t.PROBLEM1 == rec.PROBLEM1
                                        select t))
            {
                totalTests++;
                Out testResult = Test(test.Input,test.Data,test.TimeLimit,test.MemoryLimit, sock, CompileCMP.Message, ret.Message);
                switch (testResult.Type)
                {
                    case Out.ResultType.Success:
                        passedTests++;
                        rec.MemoryUsed += testResult.Memory;
                        rec.ExecutedTime += (int)testResult.Time;
                        Detail.AppendFormat("#{0}：<span class=\"score_100\"><b>通过</b></span> ({1} ms / {2} KB)<br />", totalTests, testResult.Time, testResult.Memory);
                        break;
                    case Out.ResultType.CompareError:
                        rec.Status = (int)testResult.Type;
                        Detail.AppendFormat("#{0}：<span class=\"score_0\"><b>比较器错误</b></span> (???? ms / ???? KB)<br />", totalTests);
                        break;
                    case Out.ResultType.MemoryLimitExceeded:
                        rec.Status = (int)testResult.Type;
                        Detail.AppendFormat("#{0}：<span class=\"score_0\"><b>内存超过限定</b></span> (???? ms / ???? KB)<br />", totalTests);
                        break;
                    case Out.ResultType.OutputLimitExceeded:
                        rec.Status = (int)testResult.Type;
                        Detail.AppendFormat("#{0}：<span class=\"score_0\"><b>程序吐槽过多</b></span> (???? ms / ???? KB)<br />", totalTests);
                        break;
                    case Out.ResultType.RuntimeError:
                        rec.Status = (int)testResult.Type;
                        Detail.AppendFormat("#{0}：<span class=\"score_0\"><b>运行时错误</b></span> (???? ms / ???? KB)<br />", totalTests);
                        break;
                    case Out.ResultType.TimeLimitExceeded:
                        rec.Status = (int)testResult.Type;
                        Detail.AppendFormat("#{0}：<span class=\"score_0\"><b>超时</b></span> (???? ms / ???? KB)<br />", totalTests);
                        break;
                    case Out.ResultType.WrongAnswer:
                        rec.Status = (int)testResult.Type;
                        Detail.AppendFormat("#{0}：<span class=\"score_0\"><b>答案错误</b></span> (???? ms / ???? KB)<br />", totalTests);
                        break;
                }
                if (rec.PROBLEM1.CONTEST1.Type != (int)Contest.ContestType.OI && testResult.Type != Out.ResultType.Success)
                    break;
            }
            if (totalTests == passedTests)
            {
                rec.Status = (int)Out.ResultType.Success;
            }
            rec.Score = (0 != totalTests ? passedTests / totalTests * 100 : 0);
            Detail.Append("</div>");
            rec.Detail = Detail.ToString();
        }

        void DealHunt(CHDB db, Socket sock)
        {
            var rec = (from h in db.HUNTs
                       where h.Status == (int)Hunt.StatusType.Pending
                       select h).FirstOrDefault();
            if (null == rec)
                return;

            Out DataChecker = Compile(rec.RECORD1.PROBLEM1.DataChecker, (Record.LanguageType)rec.RECORD1.PROBLEM1.DataCheckerLanguage, sock);
            if (DataChecker.Type != Out.ResultType.Success)
            {
                rec.Status = (int)Hunt.StatusType.OtherError;
                return;
            }
            Out OriRec = Compile(rec.RECORD1.Code, (Record.LanguageType)rec.RECORD1.Language, sock);
            if(OriRec.Type!=Out.ResultType.Success)
            {
                rec.Status=(int)Hunt.StatusType.OtherError;
                return;
            }
            long TimeLimit = (from t in db.TESTDATAs
                              where t.PROBLEM1 == rec.RECORD1.PROBLEM1
                              select t).Max(x => x.TimeLimit);
            long MemoryLimit = (from t in db.TESTDATAs
                                where t.PROBLEM1 == rec.RECORD1.PROBLEM1
                                select t).Max(x => x.MemoryLimit);
            Out result = Test(rec.HuntData, new byte[] { }, TimeLimit, MemoryLimit, sock, DataChecker.Message, OriRec.Message);
            if (result.Type == Out.ResultType.Success)
            {
                rec.Status = (int)Hunt.StatusType.Success;
                rec.RECORD1.Status = (int)Record.StatusType.Hacked;
            }
            else
                if (result.Type == Out.ResultType.WrongAnswer)
                    rec.Status = (int)Hunt.StatusType.Fail;
                else
                    rec.Status = (int)Hunt.StatusType.BadData;
        }

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
                    DealRecord(db, sock);
                    DealHunt(db, sock);
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