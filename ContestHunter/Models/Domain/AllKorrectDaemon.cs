﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AllKorrect;
using ContestHunter.Database;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
namespace ContestHunter.Models.Domain
{
    public class AllKorrectDaemon : Daemon
    {
        #region Compiler Infomation
        static IDictionary<Record.LanguageType,IDictionary<string,string[]>> commands=new Dictionary<Record.LanguageType,IDictionary<string,string[]>>()
        {
            {
                Record.LanguageType.CPP,
                new Dictionary<string,string[]>()
                {
                    {"compileargv",new string[]{"-O2","-o","a.out","-DONLINE_JUDGE","-Wall","-lm","--static","--std=c++11","-fno-asm","code.cpp"}},
                    {"extname",new string[]{"cpp"}},
                    {"compile",new string[]{"g++"}},
                    {"execname",new string[]{"a.out"}}
                }
            },
            {
                Record.LanguageType.C,
                new Dictionary<string,string[]>()
                {
                    {"compileargv",new string[]{"-O2","-o","a.out","-DONLINE_JUDGE","-Wall","-lm","--static","--std=c99","-fno-asm","code.c"}},
                    {"extname",new string[]{"c"}},
                    {"compile",new string[]{"gcc"}},
                    {"execname",new string[]{"a.out"}}
                }
            },
            {
                Record.LanguageType.Pascal,
                new Dictionary<string,string[]>()
                {
                    {"compileargv",new string[]{"-O1","-dONLINE_JUDGE","code.pas"}},
                    {"extname",new string[]{"pas"}},
                    {"compile",new string[]{"fpc"}},
                    {"execname",new string[]{"code"}}
                }
            },
            {
                Record.LanguageType.Java,
                new Dictionary<string,string[]>()
                {
                    {"compileargv",new string[]{"Main.java"}},
                    {"extname",new string[]{"java"}},
                    {"compile",new string[]{"javac"}},
                    {"execname",new string[]{"java"}},
                    {"execargv",new string[]{"Main"}}
                }
            }
        };
        static int CompileTime=10000;
        static long CompileMemory=256*1024*1024;

        #endregion
        #region Tester Infomation
        public static List<AKInfo> aks = new List<AKInfo>()
        {
            new AKInfo()
            {
                desp="SH-Alpha",
                ip="222.66.130.13",
                port=10010
            },
            new AKInfo()
            {
                desp="SH-Beta",
                ip="222.66.130.13",
                port=10011
            },
            new AKInfo()
            {
                desp="SH-Gamma",
                ip="222.66.130.13",
                port=10012
            },
            new AKInfo()
            {
                desp="SH-Delta",
                ip="222.66.130.13",
                port=10013
            },
            new AKInfo()
            {
                desp="SH-Epsilon",
                ip="222.66.130.13",
                port=10014
            },
            new AKInfo()
            {
                desp="SH-Zeta",
                ip="222.66.130.13",
                port=10015
            },
            new AKInfo(){
                desp="EZ-63",
                ip="192.168.13.163",
                port=10010
            },
            new AKInfo(){
                desp="Moo",
                ip="moo.imeng.de",
                port=10010
            }
        };

        #endregion

        public class AKInfo
        {
            public string ip;
            public int port;
            public string desp;
        }

        class TestInfo
        {
            public Guid hunt;
            public Guid record;
            public AKInfo ak;
            public Thread t;
        }

        public AKInfo _ak;
        public BlockingCollection<Guid> RecList = new BlockingCollection<Guid>(new ConcurrentQueue<Guid>());
        public BlockingCollection<Guid> HuntList = new BlockingCollection<Guid>(new ConcurrentQueue<Guid>());
        ExecuteResult Compile(string code,Record.LanguageType language,NativeRunner runner)
        {
            runner.PutFile("code."+commands[language]["extname"][0],Encoding.UTF8.GetBytes(code));
            return runner.Execute(commands[language]["compile"][0],commands[language]["compileargv"],CompileMemory,CompileTime,-1,RestrictionLevel.Loose,null);
        }

        bool DealRecord(NativeRunner tester,CHDB db, Guid recID, string desp)
        {
            var rec = (from r in db.RECORDs
                       where r.ID == recID
                       select r).FirstOrDefault();
            if (null == rec)
                return false;
            StringBuilder Detail = new StringBuilder("<h5 style=\"color: red; float: right; text-align: right\">测评机：" + desp + "</h5>");
            try
            {
                ExecuteResult ret = Compile(rec.Code, (Record.LanguageType)rec.Language, tester);
                if (ret.Type != ExecuteResultType.Success)
                {
                    rec.Status = (int)Record.StatusType.Compile_Error;
                    Detail.AppendFormat("<h5>编译失败：</h5>\r\n<pre style=\"padding-left: 10px\">\r\n{0}:<br />{1}</pre>", ret.Type.ToString(), Encoding.UTF8.GetString(tester.GetBlob(ret.ErrorBlob, 0, 10240)) + Encoding.UTF8.GetString(tester.GetBlob(ret.OutputBlob, 0, 10240)));
                    return true;
                }
                tester.MoveFile2File(commands[(Record.LanguageType)rec.Language]["execname"][0], "exec");
                string comparer = rec.PROBLEM1.Comparer == "" ? Resources.DefaultComparer : rec.PROBLEM1.Comparer;
                var comparerLanguage = rec.PROBLEM1.Comparer == "" ? Record.LanguageType.CPP : (Record.LanguageType)rec.PROBLEM1.ComparerLanguage;
                ExecuteResult CompileCMP = Compile(comparer, comparerLanguage, tester);
                if (CompileCMP.Type != ExecuteResultType.Success)
                {
                    rec.Status = (int)Record.StatusType.CMP_Error;
                    Detail.AppendFormat("<h5>比较器编译失败：</h5>\r\n<pre style=\"padding-left: 10px\">\r\n{0}{1}</pre>", ret.Type.ToString(), Encoding.UTF8.GetString(tester.GetBlob(ret.ErrorBlob)));
                    return true;
                }
                tester.MoveFile2File(commands[comparerLanguage]["execname"][0], "comparer");
                Detail.Append("<h5>各测试点详细信息：</h5>\r\n<div style=\"padding-left: 10px\">");
                int totalTests = 0;
                int passedTests = 0;
                rec.MemoryUsed = 0;
                rec.ExecutedTime = 0;
                Guid lastHunt;
                lock (ContestDaemon.HuntLst)
                {
                    string key = rec.User.ToString() + rec.Problem.ToString();
                    lastHunt = ContestDaemon.HuntLst.ContainsKey(key) ? ContestDaemon.HuntLst[key] : Guid.Empty;
                }
                foreach (var test in (from t in db.TESTDATAs
                                      where t.PROBLEM1.ID == rec.PROBLEM1.ID && (t.Available || t.ID == lastHunt)
                                      select new
                                      {
                                          t.ID,
                                          t.MemoryLimit,
                                          t.TimeLimit
                                      }).ToArray())
                {
                    totalTests++;
                    string inputName = test.ID.ToString() + "input";
                    string outputName = test.ID.ToString() + "output";
                    if (!tester.HasBlob(inputName))
                        tester.PutBlob(inputName, (from t in db.TESTDATAs
                                                   where t.ID == test.ID
                                                   select t.Input).Single());
                    if (!tester.HasBlob(outputName))
                        tester.PutBlob(outputName, (from t in db.TESTDATAs
                                                    where t.ID == test.ID
                                                    select t.Data).Single());
                    var result = tester.Execute("./exec", new string[] { }, test.MemoryLimit, test.TimeLimit, -1, RestrictionLevel.Strict, inputName);
                    int Time = result.UserTime;
                    int RealTime = result.RealTime;
                    long Memory_KB = result.Memory / 1024;
                    long Memory = result.Memory;
                    if (result.Type == ExecuteResultType.Success)
                    {
                        tester.CopyBlob2File(outputName, outputName);
                        tester.CopyBlob2File(result.OutputBlob, result.OutputBlob);
                        tester.CopyBlob2File(inputName, inputName);
                        result = tester.Execute("./comparer", new string[] { outputName, result.OutputBlob, inputName }, test.MemoryLimit, test.TimeLimit, 10240, RestrictionLevel.Strict, null);
                        switch (result.Type)
                        {
                            case ExecuteResultType.Success:
                                passedTests++;
                                rec.MemoryUsed = Math.Max((long)rec.MemoryUsed, Memory);
                                rec.ExecutedTime += Time;
                                Detail.AppendFormat("#{0}：<span class=\"score_100\"><b>通过</b></span> ({1} ms / {2} KB)<br />", totalTests, Time, Memory_KB);
                                break;
                            case ExecuteResultType.Failure:
                                switch (result.ExitStatus)
                                {
                                    case 1:
                                        rec.Status = (int)Record.StatusType.Wrong_Answer;
                                        Detail.AppendFormat("#{0}：<span class=\"score_0\"><b>答案错误</b></span> ({1} ms / {2} KB)<br />", totalTests, Time, Memory_KB);
                                        break;
                                    default:
                                        rec.Status = (int)Record.StatusType.CMP_Error;
                                        Detail.AppendFormat("#{0}：<span class=\"score_0\"><b>比较器错误:{1}</b></span> ({2} ms / {3} KB)<br />", totalTests, Encoding.UTF8.GetString(tester.GetBlob(result.OutputBlob)), Time, Memory_KB);
                                        break;
                                }
                                break;
                            default:
                                rec.Status = (int)Record.StatusType.CMP_Error;
                                Detail.AppendFormat("#{0}：<span class=\"score_0\"><b>比较器错误:{1}</b></span> ({2} ms / {3} KB)<br />", totalTests, Encoding.UTF8.GetString(tester.GetBlob(result.OutputBlob)), Time, Memory_KB);
                                break;
                        }
                        if (result.Type != ExecuteResultType.Success)
                            if (rec.PROBLEM1.CONTEST1.Type != (int)Contest.ContestType.OI)
                                break;
                    }
                    else
                    {
                        switch (result.Type)
                        {
                            case ExecuteResultType.MemoryLimitExceeded:
                                rec.Status = (int)Record.StatusType.Memory_Limit_Execeeded;
                                Detail.AppendFormat("#{0}：<span class=\"score_0\"><b>内存超过限定</b></span> ({1} ms / {2} KB)<br />", totalTests, Time, Memory_KB);
                                break;
                            case ExecuteResultType.Crashed:
                                rec.Status = (int)Record.StatusType.Runtime_Error;
                                Detail.AppendFormat("#{0}：<span class=\"score_0\"><b>运行时错误(程序崩溃)</b></span> ({1} ms / {2} KB)<br />", totalTests, Time, Memory_KB);
                                break;
                            case ExecuteResultType.MathError:
                                rec.Status = (int)Record.StatusType.Runtime_Error;
                                Detail.AppendFormat("#{0}：<span class=\"score_0\"><b>运行时错误(数学错误)</b></span> ({1} ms / {2} KB)<br />", totalTests, Time, Memory_KB);
                                break;
                            case ExecuteResultType.Failure:

                                rec.Status = (int)Record.StatusType.Runtime_Error;
                                Detail.AppendFormat("#{0}：<span class=\"score_0\"><b>运行时错误(返回值不为0)</b></span> ({1} ms / {2} KB)<br />", totalTests, Time, Memory_KB);
                                break;
                            case ExecuteResultType.MemoryAccessViolation:
                                rec.Status = (int)Record.StatusType.Runtime_Error;
                                Detail.AppendFormat("#{0}：<span class=\"score_0\"><b>运行时错误(内存不可访问)</b></span> ({1} ms / {2} KB)<br />", totalTests, Time, Memory_KB);
                                break;
                            case ExecuteResultType.Violation:
                                rec.Status = (int)Record.StatusType.Runtime_Error;
                                Detail.AppendFormat("#{0}：<span class=\"score_0\"><b>运行时错误(受限指令)</b></span> ({1} ms / {2} KB)<br />", totalTests, Time, Memory_KB);
                                break;
                            case ExecuteResultType.TimeLimitExceeded:
                                rec.Status = (int)Record.StatusType.Time_Limit_Execeeded;
                                Detail.AppendFormat("#{0}：<span class=\"score_0\"><b>超时</b></span> (Usertime={1} ms, Realtime={3} ms / {2} KB)<br />", totalTests, Time, Memory_KB,RealTime);
                                break;
                            case ExecuteResultType.OutputLimitExceeded:
                                rec.Status = (int)Record.StatusType.Output_Limit_Execeeded;
                                Detail.AppendFormat("#{0}：<span class=\"score_0\"><b>程序吐槽过多</b></span> ({1} ms / {2} KB)<br />", totalTests, Time, Memory_KB);
                                break;
                        }
                        if (rec.PROBLEM1.CONTEST1.Type != (int)Contest.ContestType.OI)
                            break;
                    }
                }
                if (totalTests == passedTests)
                {
                    rec.Status = (int)Record.StatusType.Accept;
                }
                rec.Score = (0 != totalTests ? passedTests * 100 / totalTests : 0);
                Detail.Append("</div>");
                return true;
            }
            finally
            {
                rec.Detail = Detail.ToString();
            }
        }

        bool DealHunt(NativeRunner tester,CHDB db, Guid huntID, string desp)
        {
            var rec = (from h in db.HUNTs
                       where h.ID == huntID
                       select h).FirstOrDefault();
            if (null == rec)
                return false;
            int TimeLimit = (from t in db.TESTDATAs
                             where t.PROBLEM1.ID == rec.RECORD1.PROBLEM1.ID
                             select t).Max(x => x.TimeLimit);
            long MemoryLimit = (from t in db.TESTDATAs
                                where t.PROBLEM1.ID == rec.RECORD1.PROBLEM1.ID
                                select t).Max(x => x.MemoryLimit);
            StringBuilder Detail = new StringBuilder("<h5 style=\"color: red; float: right; text-align: right\">测评机：" + desp + "</h5>");
            try
            {
                string HuntData = null;
                ExecuteResult result;
                switch ((Record.LanguageType)rec.DataType)
                {
                    default:
                        result = Compile(rec.HuntData, (Record.LanguageType)rec.DataType, tester);
                        if (result.Type != ExecuteResultType.Success)
                        {
                            rec.Status = (int)Hunt.StatusType.CompileError;
                            Detail.Append(Encoding.UTF8.GetString(tester.GetBlob(result.ErrorBlob, 0, 10240)));
                            break;
                        }
                        result = tester.Execute("./" + commands[(Record.LanguageType)rec.DataType]["execname"][0], new string[] { }, MemoryLimit, TimeLimit, 100 * 1024 * 1024, RestrictionLevel.Strict, null);
                        if (result.Type != ExecuteResultType.Success)
                        {
                            rec.Status = (int)Hunt.StatusType.BadData;
                            Detail.Append(Encoding.UTF8.GetString(tester.GetBlob(result.OutputBlob)));
                            break;
                        }
                        HuntData = result.OutputBlob;
                        break;
                    case Record.LanguageType.Data:
                        tester.PutBlob(HuntData = tester.RandomString(), Encoding.UTF8.GetBytes(rec.HuntData));
                        break;
                }
                if (null == HuntData)
                    return true;
                string DataChecker = rec.RECORD1.PROBLEM1.DataChecker;
                result = Compile(DataChecker, (Record.LanguageType)rec.RECORD1.PROBLEM1.DataCheckerLanguage, tester);
                if (result.Type != ExecuteResultType.Success)
                {
                    rec.Status = (int)Hunt.StatusType.DataCheckerError;
                    Detail.Append(Encoding.UTF8.GetString(tester.GetBlob(result.ErrorBlob)));
                    return true;
                }
                result = tester.Execute("./" + commands[(Record.LanguageType)rec.RECORD1.PROBLEM1.DataCheckerLanguage]["execname"][0], new string[] { }, MemoryLimit, TimeLimit, 100 * 1024 * 1024, RestrictionLevel.Strict, HuntData);
                if (result.Type != ExecuteResultType.Success)
                {
                    if (result.Type == ExecuteResultType.Failure && result.ExitStatus == 1)
                        rec.Status = (int)Hunt.StatusType.BadData;
                    else
                        rec.Status = (int)Hunt.StatusType.DataCheckerError;
                    Detail.Append(result.Type.ToString() + Encoding.UTF8.GetString(tester.GetBlob(result.OutputBlob)));
                    return true;
                }
                string stdout = result.OutputBlob;
                result = Compile(rec.RECORD1.Code, (Record.LanguageType)rec.RECORD1.Language, tester);
                if (result.Type != ExecuteResultType.Success)
                {
                    rec.Status = (int)Hunt.StatusType.OtherError;
                    Detail.Append("原记录编译失败");
                    return true;
                }
                result = tester.Execute("./" + commands[(Record.LanguageType)rec.RECORD1.Language]["execname"][0], new string[] { }, MemoryLimit, TimeLimit, 100 * 1024 * 1024, RestrictionLevel.Strict, HuntData);
                tester.MoveBlob2File(stdout, stdout);
                tester.MoveBlob2File(HuntData, HuntData);
                if (result.Type == ExecuteResultType.Success)
                {
                    string userout = result.OutputBlob;
                    string comparer = rec.RECORD1.PROBLEM1.Comparer == "" ? Resources.DefaultComparer : rec.RECORD1.PROBLEM1.Comparer;
                    var comparerLanguage = rec.RECORD1.PROBLEM1.Comparer == "" ? Record.LanguageType.CPP : (Record.LanguageType)rec.RECORD1.PROBLEM1.ComparerLanguage;
                    result = Compile(comparer, comparerLanguage, tester);
                    if (result.Type != ExecuteResultType.Success)
                    {
                        rec.Status = (int)Hunt.StatusType.OtherError;
                        Detail.Append("比较器编译失败");
                        return true;
                    }
                    tester.MoveBlob2File(userout, userout);
                    result = tester.Execute("./" + commands[comparerLanguage]["execname"][0], new string[] { stdout, userout, HuntData }, MemoryLimit, TimeLimit, 10 * 1024, RestrictionLevel.Strict, null);
                    if (result.Type == ExecuteResultType.Failure && result.ExitStatus == 1)
                    {
                        var newid = Guid.NewGuid();
                        rec.RECORD1.PROBLEM1.TESTDATAs.Add(new TESTDATA()
                        {
                            ID = newid,
                            Available = false,
                            MemoryLimit = MemoryLimit,
                            TimeLimit = TimeLimit,
                            Data = tester.GetFile(stdout),
                            Input = tester.GetFile(HuntData)
                        });
                        rec.RECORD1.Status = (int)Record.StatusType.Hacked;
                        rec.Status = (int)Hunt.StatusType.Success;
                        lock (ContestDaemon.HuntLst)
                        {
                            string key = rec.RECORD1.User.ToString() + rec.RECORD1.Problem.ToString();
                            if (!ContestDaemon.HuntLst.ContainsKey(key))
                                ContestDaemon.HuntLst.Add(key, newid);
                            else
                                ContestDaemon.HuntLst[key] = newid;
                        }
                        return true;
                    }
                    else if (result.Type == ExecuteResultType.Success)
                    {
                        rec.Status = (int)Hunt.StatusType.Fail;
                        return true;
                    }
                    else
                    {
                        rec.Status = (int)Hunt.StatusType.OtherError;
                        Detail.Append("比较器错误");
                        return true;
                    }
                }
                else
                {
                    Guid newid = Guid.NewGuid();
                    rec.RECORD1.PROBLEM1.TESTDATAs.Add(new TESTDATA()
                    {
                        ID = newid,
                        Available = false,
                        MemoryLimit = MemoryLimit,
                        TimeLimit = TimeLimit,
                        Data = tester.GetFile(stdout),
                        Input = tester.GetFile(HuntData)
                    });
                    rec.RECORD1.Status = (int)Record.StatusType.Hacked;
                    rec.Status = (int)Hunt.StatusType.Success;
                    lock (ContestDaemon.HuntLst)
                    {
                        string key = rec.RECORD1.User.ToString() + rec.RECORD1.Problem.ToString();
                        if (!ContestDaemon.HuntLst.ContainsKey(key))
                            ContestDaemon.HuntLst.Add(key, newid);
                        else
                            ContestDaemon.HuntLst[key] = newid;
                    }
                    return true;
                }
            }
            finally
            {
                rec.Detail = Detail.ToString();
            }
        }


        protected override int Run()
        {
            bool flg=false;
            Guid Record,Hunt;
            flg |= RecList.TryTake(out Record);
            flg |= HuntList.TryTake(out Hunt);
            if (!flg)
            {
                return 3000;
            }
            using (var db = new CHDB())
            {
                try
                {
                    using (var tester = new NativeRunner(_ak.ip, _ak.port))
                    {
                        DealRecord(tester, db, Record, _ak.desp);
                        db.SaveChanges();
                        DealHunt(tester, db, Hunt, _ak.desp);
                        db.SaveChanges();
                    }
                }
                catch
                {
                    if (Record != Guid.Empty)
                        (from h in db.RECORDs
                         where h.ID == Record
                         select h).Single().Status = (int)Domain.Record.StatusType.Pending;
                    if (Hunt != Guid.Empty)
                        (from h in db.HUNTs
                         where h.ID == Hunt
                         select h).Single().Status = (int)Domain.Hunt.StatusType.Pending;
                    db.SaveChanges();
                    throw;
                }
            }
            return 0;
        }
    }
}