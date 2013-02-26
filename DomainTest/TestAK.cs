using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AllKorrect;
using System.Text;
namespace DomainTest
{
    [TestClass]
    public class TestAK
    {
        const string HOST = "222.66.130.13";
        const int PORT = 10010;

        [TestMethod]
        public void TestConnection()
        {
            using (NativeRunner runner = new NativeRunner(HOST, PORT))
            {
            }
        }

        [TestMethod]
        public void TestEmptyInput()
        {
            using (NativeRunner runner = new NativeRunner(HOST, PORT))
            {
                var result = runner.Execute("echo", new string[] { "123" }, -1, -1, -1, RestrictionLevel.Loose, null);
                Assert.AreEqual(ExecuteResultType.Success, result.Type);
                Assert.IsTrue(Encoding.UTF8.GetString(runner.GetBlob(result.OutputBlob)).Contains("123"));
            }
        }

        [TestMethod]
        public void TestPutGetFile()
        {
            using (NativeRunner runner = new NativeRunner(HOST, PORT))
            {
                string data = "Hello AK";
                runner.PutFile("data", Encoding.ASCII.GetBytes(data));
                Assert.IsTrue(runner.HasFile("data"));
                Assert.AreEqual(data, Encoding.ASCII.GetString(runner.GetFile("data")));
            }
        }

        [TestMethod]
        public void TestSimpleCPP()
        {
            using (NativeRunner runner = new NativeRunner(HOST, PORT))
            {
                string src = "#include <cstdio>\n"
                    + "int main(){"
                    + "  int x,y;"
                    + "  scanf(\"%d%d\",&x,&y);"
                    + "  printf(\"%d\",x+y);"
                    + "}";
                string input = "1 2";

                runner.PutFile("src.cpp", Encoding.UTF8.GetBytes(src));
                runner.PutBlob("input", Encoding.UTF8.GetBytes(input));

                var result = runner.Execute("g++", new[] { "-o", "exe", "src.cpp" }, 100 * 1024 * 1024, 3000, 10 * 1024 * 1024, RestrictionLevel.Loose, null);
                Assert.AreEqual(ExecuteResultType.Success, result.Type);

                result = runner.Execute("./exe", new string[] { }, 100 * 1024 * 1024, 1000, 10, RestrictionLevel.Strict, "input");
                Assert.AreEqual(ExecuteResultType.Success, result.Type);

                var output = Encoding.UTF8.GetString(runner.GetBlob(result.OutputBlob));
                Assert.AreEqual("3", output);
            }
        }

        [TestMethod]
        public void TestSimpleC()
        {
            using (NativeRunner runner = new NativeRunner(HOST, PORT))
            {
                string src = "#include <stdio.h>\n"
                    + "int main(){"
                    + "  int x,y;"
                    + "  scanf(\"%d%d\",&x,&y);"
                    + "  printf(\"%d\",x+y);"
                    + "  return 0;"
                    + "}";
                string input = "1 2";

                runner.PutFile("src.c", Encoding.UTF8.GetBytes(src));
                runner.PutBlob("input", Encoding.UTF8.GetBytes(input));

                var result = runner.Execute("gcc", new[] { "-o", "exe", "src.c" }, 100 * 1024 * 1024, 3000, 10 * 1024 * 1024, RestrictionLevel.Loose, null);
                Assert.AreEqual(ExecuteResultType.Success, result.Type);

                result = runner.Execute("./exe", new string[] { }, 100 * 1024 * 1024, 1000, 10, RestrictionLevel.Strict, "input");
                Assert.AreEqual(ExecuteResultType.Success, result.Type);

                var output = Encoding.UTF8.GetString(runner.GetBlob(result.OutputBlob));
                Assert.AreEqual("3", output);
            }
        }

        [TestMethod]
        public void TestHas()
        {
            using (NativeRunner runner = new NativeRunner(HOST, PORT))
            {
                runner.PutBlob("blob", Encoding.ASCII.GetBytes("Hello"));
                Assert.IsTrue(runner.HasBlob("blob"));
                Assert.AreEqual("Hello", Encoding.ASCII.GetString(runner.GetBlob("blob")));

                runner.PutFile("file", Encoding.ASCII.GetBytes("Hello2"));
                Assert.IsTrue(runner.HasFile("file"));
                Assert.AreEqual("Hello2", Encoding.ASCII.GetString(runner.GetFile("file")));
            }
        }

        [TestMethod]
        public void TestMove()
        {
            using (NativeRunner runner = new NativeRunner(HOST, PORT))
            {
                runner.PutBlob("blob", new byte[0]);
                Assert.IsTrue(runner.HasBlob("blob"));

                runner.MoveBlob2Blob("blob", "blob2");
                Assert.IsTrue(runner.HasBlob("blob2"));
                Assert.IsFalse(runner.HasBlob("blob"));

                runner.MoveBlob2File("blob2", "file");
                Assert.IsFalse(runner.HasBlob("blob2"));
                Assert.IsTrue(runner.HasFile("file"));

                runner.MoveFile2File("file", "file2");
                Assert.IsTrue(runner.HasFile("file2"));
                Assert.IsFalse(runner.HasFile("file"));

                runner.MoveFile2Blob("file2", "blob3");
                Assert.IsTrue(runner.HasBlob("blob3"));
                Assert.IsFalse(runner.HasFile("file2"));
            }
        }

        [TestMethod]
        public void TestCopy()
        {
            using (NativeRunner runner = new NativeRunner(HOST, PORT))
            {
                runner.PutBlob("blob", new byte[0]);
                Assert.IsTrue(runner.HasBlob("blob"));

                runner.CopyBlob2Blob("blob", "blob2");
                Assert.IsTrue(runner.HasBlob("blob2"));
                Assert.IsTrue(runner.HasBlob("blob"));

                runner.CopyBlob2File("blob2", "file");
                Assert.IsTrue(runner.HasBlob("blob2"));
                Assert.IsTrue(runner.HasFile("file"));

                runner.CopyFile2File("file", "file2");
                Assert.IsTrue(runner.HasFile("file2"));
                Assert.IsTrue(runner.HasFile("file"));

                runner.CopyFile2Blob("file2", "blob3");
                Assert.IsTrue(runner.HasBlob("blob3"));
                Assert.IsTrue(runner.HasFile("file2"));
            }
        }

        [TestMethod]
        public void TestOverwrite()
        {
            using (NativeRunner runner = new NativeRunner(HOST, PORT))
            {
                runner.PutBlob("blob", Encoding.ASCII.GetBytes("Hello"));
                Assert.AreEqual("Hello", Encoding.ASCII.GetString(runner.GetBlob("blob")));
                runner.PutBlob("blob", Encoding.ASCII.GetBytes("Hi"));
                Assert.AreEqual("Hi", Encoding.ASCII.GetString(runner.GetBlob("blob")));
            }
        }

        [TestMethod]
        public void TestSimpleJava()
        {
            using (NativeRunner runner = new NativeRunner(HOST, PORT))
            {
                string src = ""
                    + "import java.util.*;"
                    + "public class Main{"
                    + "  public static void main(String[] args){"
                    + "    Scanner sc=new Scanner(System.in);"
                    + "    System.out.print(sc.nextInt()+sc.nextInt());"
                    + "  }"
                    + "}";
                string input = "1 2";
                runner.PutBlob("input", Encoding.ASCII.GetBytes(input));
                runner.PutFile("Main.java", Encoding.ASCII.GetBytes(src));
                var result = runner.Execute("javac", new[] { "Main.java" }, 2L * 1024 * 1024 * 1024, 3000, 100 * 1024 * 1024, RestrictionLevel.Loose, null);
                Assert.AreEqual(ExecuteResultType.Success, result.Type);

                result = runner.Execute("java", new[] { "Main" }, 4L * 1024 * 1024 * 1024, 2000, 100 * 1024 * 1024, RestrictionLevel.Loose, "input");
                var output = Encoding.ASCII.GetString(runner.GetBlob(result.OutputBlob));
                var error = Encoding.ASCII.GetString(runner.GetBlob(result.ErrorBlob));
                Assert.AreEqual(ExecuteResultType.Success, result.Type);
                Assert.AreEqual("3", output);
            }
        }

        [TestMethod]
        public void TestSimplePython()
        {
            using (NativeRunner runner = new NativeRunner(HOST, PORT))
            {
                string src = "print(raw_input())";
                string input = "Hello Python";
                runner.PutBlob("input", Encoding.ASCII.GetBytes(input));
                runner.PutFile("src.py", Encoding.ASCII.GetBytes(src));
                var result = runner.Execute("python", new[] { "src.py" }, 100 * 1024 * 1024, 1000, 10 * 1024 * 1024, RestrictionLevel.Loose, "input");
                var output = Encoding.ASCII.GetString(runner.GetBlob(result.OutputBlob));
                var error = Encoding.ASCII.GetString(runner.GetBlob(result.ErrorBlob));
                Assert.AreEqual(ExecuteResultType.Success, result.Type);
                Assert.AreEqual(input + "\n", output);
            }
        }

        [TestMethod]
        public void TestSimplePascal()
        {
            using (NativeRunner runner = new NativeRunner(HOST, PORT))
            {
                string src = ""
                    + "var"
                    + "  x,y:longint;"
                    + "begin"
                    + "  read(x,y);"
                    + "  write(x+y);"
                    + "end.";
                string input = "1 2";
                runner.PutFile("src.pas", Encoding.ASCII.GetBytes(src));
                runner.PutBlob("input", Encoding.ASCII.GetBytes(input));
                var result = runner.Execute("fpc", new[] { "src.pas" }, 100 * 1024 * 1024, 3000, 10 * 1024 * 1024, RestrictionLevel.Loose, null);
                Assert.AreEqual(ExecuteResultType.Success, result.Type);

                result = runner.Execute("./src", new string[] { }, 100 * 1024 * 1024, 1000, 10 * 1024 * 1024, RestrictionLevel.Strict, "input");
                var output = Encoding.ASCII.GetString(runner.GetBlob(result.OutputBlob));
                Assert.AreEqual(ExecuteResultType.Success, result.Type);
                Assert.AreEqual("3", output);
            }
        }

        [TestMethod]
        public void TestDoublePut()
        {
            using (NativeRunner runner = new NativeRunner(HOST, PORT))
            {
                runner.PutFile("code.cpp", Encoding.UTF8.GetBytes("int main(){}"));
                runner.Execute("g++", new string[] { "-O2", "-o", "a.out","code.cpp" }, 64 * 1024 * 1024, 1000, 100 * 1024, RestrictionLevel.Loose, null);
                runner.MoveFile2File("a.out", "exec");
                runner.PutFile("code.cpp", Encoding.UTF8.GetBytes("哈哈"));
            }
        }
    }
}
