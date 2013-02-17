using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Configuration;
using ICSharpCode.SharpZipLib.Zip;
using ContestHunter.Models.Domain;
using ContestHunter.Models.View;
using USER = ContestHunter.Models.Domain.User;

namespace ContestHunter.Controllers
{
    public class ProblemController : Controller
    {
        static readonly int DEFAULT_TEST_CASE_MEMORY_LIMIT = int.Parse(ConfigurationManager.AppSettings["TestCase.DefaultMemoryLimit"]);
        static readonly int DEFAULT_TEST_CASE_TIME_LIMIT = int.Parse(ConfigurationManager.AppSettings["TestCase.DefaultTimeLimit"]);

        public ActionResult Show(string id, string contest)
        {
            Problem problem;
            try
            {
                Contest con = Contest.ByName(contest);
                problem = con.ProblemByName(id);
                if (problem.Content == null)
                {
                    problem.Content = "<span style='color:red;'>无法查看题目内容</span>";
                }
                try
                {
                    var testCases = from tid in problem.TestCases()
                                    select problem.TestCaseByID(tid);

                    ViewBag.TotalTimeLimit = testCases.Select(t => t.TimeLimit).DefaultIfEmpty().Sum();
                    ViewBag.MinimumMemoryLimit = testCases.Select(t => t.MemoryLimit).DefaultIfEmpty().Min();
                    ViewBag.MaximumMemoryLimit = testCases.Select(t => t.MemoryLimit).DefaultIfEmpty().Max();
                }
                catch
                {
                    ViewBag.TotalTimeLimit = -1;
                    ViewBag.MinimumMemoryLimit = -1;
                    ViewBag.MaximumMemoryLimit = -1;
                }
                ViewBag.CanEdit = con.Owners.Contains(USER.CurrentUserName);
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛不存在" });
            }
            catch (ProblemNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "题目不存在" });
            }
            return View(problem);
        }

        [HttpGet]
        public ActionResult Submit(string id, string contest)
        {
            ProblemSubmitModel model = new ProblemSubmitModel();
            model.Problem = id;
            model.Contest = contest;

            Problem problem;
            try
            {
                problem = Contest.ByName(contest).ProblemByName(id);
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛不存在" });
            }
            catch (ProblemNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "题目不存在" });
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Submit(string id, string contest, ProblemSubmitModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            Guid recordID;
            try
            {
                Problem problem = Contest.ByName(contest).ProblemByName(id);
                recordID = problem.Submit(new Record
                {
                    Code = model.Code,
                    Language = (Record.LanguageType)model.Language
                });
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛不存在" });
            }
            catch (ContestNotStartedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛尚未开始，不能提交代码" });
            }
            catch (NotAttendedContestException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有报名比赛，不能提交代码" });
            }
            catch (UserNotLoginException)
            {
                throw;
            }
            catch (ProblemNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "题目不存在" });
            }
            catch (ProblemLockedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "题目被锁定，无法提交代码" });
            }

            return RedirectToAction("Show", "Record", new { id = recordID });
        }

        [HttpGet]
        public ActionResult Add(string contest)
        {
            ProblemBasicInfoModel model = new ProblemBasicInfoModel
            {
                Contest = contest
            };
            try
            {
                Contest con = Contest.ByName(contest);
                model.ContestType = con.Type;
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有这场比赛" });
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Add(string contest, ProblemBasicInfoModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                Contest con = Contest.ByName(contest);
                if (con.Type == Contest.ContestType.CF && model.OriginalRating == null)
                {
                    ModelState.AddModelError("OriginalRating", "不可为空");
                    return View(model);
                }
                con.AddProblem(new Problem
                {
                    Content = System.IO.File.ReadAllText(Server.MapPath("~/Content/ProblemTemplate.html")),
                    Name = model.Name,
                    Comparer = "",
                    DataChecker = "",
                    ComparerLanguage = Record.LanguageType.CPP,
                    DataCheckerLanguage = Record.LanguageType.CPP,
                    OriginRating = model.OriginalRating,
                    Owner = model.Owner
                });
            }
            catch (ProblemNameExistedException)
            {
                ModelState.AddModelError("Name", "题目名已存在");
                return View(model);
            }
            catch (UserNotFoundException)
            {
                ModelState.AddModelError("Owner", "没有这个用户");
                return View(model);
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没找到相应比赛" });
            }
            catch (PermissionDeniedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有添加比赛权限" });
            }
            catch (UserNotLoginException)
            {
                throw;
            }
            return RedirectToAction("Description", new { id = model.Name, contest = contest });
        }

        [HttpGet]
        public ActionResult Edit(string id, string contest)
        {
            ProblemBasicInfoModel model = new ProblemBasicInfoModel
            {
                Contest = contest,
                Problem = id
            };
            try
            {
                Contest con = Contest.ByName(contest);
                Problem problem = con.ProblemByName(id);
                model.Owner = problem.Owner;
                model.OriginalRating = problem.OriginRating;
                model.ContestType = con.Type;
                model.Name = problem.Name;
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有找到相应的比赛" });
            }
            catch (ProblemNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有这个题目" });
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(string id, string contest, ProblemBasicInfoModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                Contest con = Contest.ByName(contest);
                if (con.Type == Contest.ContestType.CF && model.OriginalRating == null)
                {
                    ModelState.AddModelError("OriginalRating", "不可为空");
                    return View(model);
                }
                Problem problem = con.ProblemByName(id);
                problem.Name = model.Name;
                problem.OriginRating = model.OriginalRating;
                problem.Owner = model.Owner;
                problem.Change();
            }
            catch (ProblemNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有这个题目" });
            }
            catch (ProblemNameExistedException)
            {
                ModelState.AddModelError("Name", "题目名已存在");
                return View(model);
            }
            catch (UserNotFoundException)
            {
                ModelState.AddModelError("Owner", "没有这个用户");
                return View(model);
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没找到相应比赛" });
            }
            catch (PermissionDeniedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有添加比赛权限" });
            }
            catch (UserNotLoginException)
            {
                throw;
            }
            return RedirectToAction("Description", new { id = model.Name, contest = contest });
        }

        [HttpGet]
        public ActionResult Description(string contest, string id)
        {
            ProblemContentModel model = new ProblemContentModel
            {
                Contest = contest,
                Problem = id
            };
            try
            {
                Problem problem = Contest.ByName(contest).ProblemByName(id);
                if (problem.Content == null)
                {
                    return RedirectToAction("Error", "Shared", new { msg = "无法查看题目内容" });
                }
                model.Content = problem.Content;
                model.ProblemOwner = problem.Owner;
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有找到所需的比赛" });
            }
            catch (ProblemNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有找到所需的题目" });
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Description(string id, string contest, ProblemContentModel model)
        {
            if (!ModelState.IsValid) return View(model);
            switch (model.Action)
            {
                case ProblemContentModel.ActionType.Preview:
                    return View(model);
                case ProblemContentModel.ActionType.Modify:
                    try
                    {
                        Problem problem = Contest.ByName(contest).ProblemByName(id);
                        problem.Content = model.Content;
                        problem.Change();
                    }
                    catch (ContestNotFoundException)
                    {
                        return RedirectToAction("Error", "Shared", new { msg = "没有找到相应比赛" });
                    }
                    catch (ProblemNotFoundException)
                    {
                        return RedirectToAction("Error", "Shared", new { msg = "没有找到相应题目" });
                    }
                    catch (PermissionDeniedException)
                    {
                        return RedirectToAction("Error", "Shared", new { msg = "没有权限修改题目内容" });
                    }
                    catch (UserNotLoginException)
                    {
                        throw;
                    }
                    catch (UserNotFoundException)
                    {
                        throw;
                    }
                    return RedirectToAction("TestCase", new { id = id, contest = contest });
            }
            throw new NotImplementedException();
        }

        #region TestCase
        TestCaseInfo TestCase2Info(TestCase t)
        {
            return new TestCaseInfo
            {
                ID = t.ID,
                Memory = t.MemoryLimit / (double)(1024 * 1024),
                Time = t.TimeLimit / 1000.0,
                InputHash = new CRC32().AsString(t.Input),
                OutputHash = new CRC32().AsString(t.Data),
                InputSize = t.Input.Length,
                OutputSize = t.Data.Length,
                Input = Encoding.Default.GetString(t.Input.Take(100).ToArray()),
                Output = Encoding.Default.GetString(t.Data.Take(100).ToArray())
            };
        }

        [HttpGet]
        public ActionResult TestCase(string id, string contest)
        {
            TestCaseUploadModel model = new TestCaseUploadModel
            {
                Contest = contest,
                Problem = id
            };
            try
            {
                Contest con = Contest.ByName(contest);
                Problem problem = con.ProblemByName(id);
                model.TestCases = (from t in problem.TestCases()
                                   select TestCase2Info(problem.TestCaseByID(t))).ToList();
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有找到相关比赛" });
            }
            catch (ProblemNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有找到相关题目" });
            }
            return View(model);
        }

        bool FoundEntry(string filename, byte[] bytes, Dictionary<int, byte[]> inputFiles, Dictionary<int, byte[]> outputFiles)
        {
            string name = Path.GetFileName(filename);
            Match match = Regex.Match(name, @"(\d+)\.in(put)?$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                int fileID = int.Parse(match.Groups[1].Value);
                if (inputFiles.ContainsKey(fileID))
                {
                    ModelState.AddModelError("File", "存在编号为" + fileID + "的多个输入文件");
                    return false;
                }
                inputFiles.Add(fileID, bytes);
            }
            else
            {
                match = Regex.Match(name, @"(\d+)\.out?(put)?$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    int fileID = int.Parse(match.Groups[1].Value);
                    if (outputFiles.ContainsKey(fileID))
                    {
                        ModelState.AddModelError("File", "存在编号为" + fileID + "的多个输出文件");
                        return false;
                    }
                    outputFiles.Add(fileID, bytes);
                }
                else
                {
                    ModelState.AddModelError("File", "无法识别的文件名:" + filename + "。请参照下方的命名规范命名。");
                    return false;
                }
            }
            return true;
        }

        List<TestCase> AddTestCase(Problem problem, HttpPostedFileBase file)
        {
            if (file == null)
            {
                ModelState.AddModelError("File", "请选择文件");
                return null;
            }
            Dictionary<int, byte[]> inputFiles = new Dictionary<int, byte[]>();
            Dictionary<int, byte[]> outputFiles = new Dictionary<int, byte[]>();

            if (new[] { "application/zip", "application/x-zip-compressed" }.Contains(file.ContentType))
            {
                using (ZipInputStream stream = new ZipInputStream(file.InputStream))
                {
                    ZipEntry entry;
                    while ((entry = stream.GetNextEntry()) != null)
                    {
                        byte[] bytes;
                        using (MemoryStream mem = new MemoryStream())
                        {
                            stream.CopyTo(mem);
                            bytes = mem.ToArray();
                        }
                        if (!FoundEntry(entry.Name, bytes, inputFiles, outputFiles))
                        {
                            return null;
                        }
                    }
                }
            }
            else
            {
                ModelState.AddModelError("File", "不支持的压缩文件类型");
                return null;
            }

            if (!inputFiles.Keys.OrderBy(x => x).SequenceEqual(outputFiles.Keys.OrderBy(x => x)))
            {
                ModelState.AddModelError("File", "输入与输出文件没有一一对应");
                return null;
            }

            var testCases = inputFiles.Keys.Select(id => new TestCase
                {
                    Input = inputFiles[id],
                    Data = outputFiles[id],
                    MemoryLimit = DEFAULT_TEST_CASE_MEMORY_LIMIT,
                    TimeLimit = DEFAULT_TEST_CASE_TIME_LIMIT
                }).ToList();
            foreach (var t in testCases)
            {
                t.ID = problem.AddTestCase(t);
            }
            return testCases;
        }

        [HttpPost]
        public ActionResult TestCase(string id, string contest, TestCaseUploadModel model)
        {
            if (model.TestCases == null)
            {
                model.TestCases = new List<TestCaseInfo>();
            }
            if (!ModelState.IsValid) return View(model);
            ModelState.Clear();

            Problem problem = Contest.ByName(contest).ProblemByName(id);
            foreach (var testCase in model.TestCases)
            {
                ContestHunter.Models.Domain.TestCase.Change(testCase.ID, (int)(testCase.Time * 1000), (int)(testCase.Memory * 1024 * 1024));
            }
            switch (model.Action)
            {
                case TestCaseUploadModel.ActionType.Upload:
                    var newCases = AddTestCase(problem, model.File);
                    if (newCases != null)
                    {
                        foreach (var t in newCases.Select(TestCase2Info))
                        {
                            model.TestCases.Add(t);
                        }
                    }
                    break;
                case TestCaseUploadModel.ActionType.Next:
                    return RedirectToAction("Program", new { id = id, contest = contest });
                case TestCaseUploadModel.ActionType.Delete:
                    problem.RemoveTestCase(model.TestCases[model.TestCaseIndex].ID);
                    model.TestCases.RemoveAt(model.TestCaseIndex);
                    break;
            }
            return View(model);
        }
        #endregion

        [HttpGet]
        public ActionResult Program(string id, string contest)
        {
            ProblemProgramModel model = new ProblemProgramModel
            {
                Contest = contest,
                Problem = id
            };
            try
            {
                Problem problem = Contest.ByName(contest).ProblemByName(id);
                model.Spj = problem.Comparer;
                model.Std = problem.DataChecker;
                model.SpjLanguage = problem.ComparerLanguage.Value;
                model.StdLanguage = problem.DataCheckerLanguage.Value;
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "找不到相关比赛" });
            }
            catch (ProblemNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "找不到相关题目" });
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Program(string id, string contest, ProblemProgramModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                Problem problem = Contest.ByName(contest).ProblemByName(id);
                problem.Comparer = model.Spj;
                problem.DataChecker = model.Std;
                problem.DataCheckerLanguage = model.StdLanguage;
                problem.ComparerLanguage = model.SpjLanguage;
                problem.Change();
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "找不到相关比赛" });
            }
            catch (ProblemNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "找不到相关题目" });
            }

            return RedirectToAction("Complete", new { id = id, contest = contest });
        }

        public ActionResult Complete(string id, string contest)
        {
            ViewBag.Contest = contest;
            ViewBag.Problem = id;
            return View();
        }

        public ActionResult TestCaseList(string id, string contest)
        {
            TestCaseListModel model = new TestCaseListModel
            {
                Problem = id,
                Contest = contest
            };
            Problem problem = Contest.ByName(contest).ProblemByName(id);
            model.TestCases = problem.TestCases().Select(problem.TestCaseByID).Select(TestCase2Info).ToList();
            return View(model);
        }

        public ActionResult TestCaseDownload(string id, string contest, Guid testCaseID)
        {
            try
            {
                TestCase testCase = Contest.ByName(contest).ProblemByName(id).TestCaseByID(testCaseID);
                using (MemoryStream mem = new MemoryStream())
                {
                    using (ZipOutputStream zip = new ZipOutputStream(mem))
                    {
                        zip.PutNextEntry(new ZipEntry("Input"));
                        zip.Write(testCase.Input, 0, testCase.Input.Length);
                        zip.PutNextEntry(new ZipEntry("Output"));
                        zip.Write(testCase.Data, 0, testCase.Data.Length);
                    }
                    return File(mem.ToArray(), "application/zip", "TestCase" + testCase.ID);
                }
            }
            catch (ContestNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有找到相应比赛" });
            }
            catch (ProblemNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有找到相应题目" });
            }
            catch (UserNotLoginException)
            {
                throw;
            }
            catch (ContestNotEndedException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "比赛尚未结束，不可下载数据" });
            }
            catch (TestCaseNotFoundException)
            {
                return RedirectToAction("Error", "Shared", new { msg = "没有找到相应的测试数据" });
            }
        }
    }
}
