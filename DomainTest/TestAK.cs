using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AllKorrect;
using System.Text;
namespace DomainTest
{
    [TestClass]
    public class TestAK
    {
        const string HOST = "moo.imeng.de";
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
        public void TestLongFile()
        {
            using (NativeRunner runner = new NativeRunner(HOST, PORT))
            {
                string longFile = "Linus大婶在slashdot上回答一些编程爱好者的提问，其中一个人问他什么样的代码是他所喜好的，大婶表述了自己一些观点之后，举了一个指针的例子，解释了什么才是core low-level coding。下面是Linus的教学原文及翻译——"
                    + "“At the opposite end of the spectrum, I actually wish more people understood the really core low-level kind of coding. Not big, complex stuff like the lockless name lookup, but simply good use of pointers-to-pointers etc. For example, I’ve seen too many people who delete a singly-linked list entry by keeping track of the “prev” entry, and then to delete the entry, doing something like。（在这段话的最后，我实际上希望更多的人了解什么是真正的核心底层代码。这并不像无锁文件名查询（注：可能是git源码里的设计）那样庞大、复杂，只是仅仅像诸如使用二级指针那样简单的技术。例如，我见过很多人在删除一个单项链表的时候，维护了一个”prev”表项指针，然后删除当前表项，就像这样）”"
                    + "and whenever I see code like that, I just go “This person doesn’t understand pointers”. And it’s sadly quite common.（当我看到这样的代码时，我就会想“这个人不了解指针”。令人难过的是这太常见了。）"
                    + "People who understand pointers just use a “pointer to the entry pointer”, and initialize that with the address of the list_head. And then as they traverse the list, they can remove the entry without using any conditionals, by just doing a “*pp = entry->next”. （了解指针的人会使用链表头的地址来初始化一个“指向节点指针的指针”。当遍历链表的时候，可以不用任何条件判断（注：指prev是否为链表头）就能移除某个节点，只要写)"
                    + "So there’s lots of pride in doing the small details right. It may not be big and important code, but I do like seeing code where people really thought about the details, and clearly also were thinking about the compiler being able to generate efficient code (rather than hoping that the compiler is so smart that it can make efficient code *despite* the state of the original source code). （纠正细节是令人自豪的事。也许这段代码并非庞大和重要，但我喜欢看那些注重代码细节的人写的代码，也就是清楚地了解如何才能编译出有效代码（而不是寄望于聪明的编译器来产生有效代码，即使是那些原始的汇编代码））。"
                    + "Linus举了一个单向链表的例子，但给出的代码太短了，一般的人很难搞明白这两个代码后面的含义。正好，有个编程爱好者阅读了这段话，并给出了一个比较完整的代码。他的话我就不翻译了，下面给出代码说明。";
                runner.PutFile("code.cpp", Encoding.UTF8.GetBytes(longFile));
                Assert.AreEqual(longFile, Encoding.UTF8.GetString(runner.GetFile("code.cpp")));
            }
        }
    }
}
