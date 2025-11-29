using QuickCode.Compiler;
using System.Reflection.Emit;
using System.Reflection;
using Mono.Cecil;

namespace QuickCode.IntegrationTest;

[TestClass]
public sealed class TestQuickCodeIntegration
{
    [TestMethod]
    [DoNotParallelize]
    public void TestSimpleBinary()
    {
        Test("""
            Print(1 + 1)
            """, expectedOutput: "2\n");
        Test("""
            Print(1 - 1)
            """, expectedOutput: "0\n");
        Test("""
            Print(1 * 1)
            """, expectedOutput: "1\n");
        Test("""
            Print(6 / 2)
            """, expectedOutput: "3\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestSimplePrecedence()
    {
        Test("""
            Print(1 + 1 * 2)
            """, expectedOutput: "3\n");
        Test("""
            Print(1 + 5 / 2)
            """, expectedOutput: "3\n");
        Test("""
            Print(2 * 3 / 2)
            """, expectedOutput: "3\n");
        Test("""
            Print(1 + 2 - 3)
            """, expectedOutput: "0\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestMultiStatements()
    {
        Test("""
            Print(1)
            Print(2)
            Print(3)
            """, expectedOutput: "1\n2\n3\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestWriteLocalVariable()
    {
        Test("""
            a : int = 123 // do not die
            """);
        Test("""
            a : int = 123
            Print(a)
            """, expectedOutput: "123\n");
        Test("""
            a : int = 123
            Print(a)
            a = 456
            Print(a)
            """, expectedOutput: "123\n456\n");
        Test("""
            a : int = 123
            Print(a)
            a = 456 // don't out of order
            """, expectedOutput: "123\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestReadWriteLocalVariable()
    {
        Test("""
            a : int = 123
            a = a
            Print(a)
            """, expectedOutput: "123\n");
        Test("""
            a : int = 123
            Print(a + 1)
            """, expectedOutput: "124\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestLocalVariable()
    {
        Test("""
            a : int = 0
            Print(a)
            a = a + 2
            Print(a)
            a = 12 / 2
            Print(a)
            a = 123
            Print(a)
            """, expectedOutput: "0\n2\n6\n123\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestPrecedence()
    {
        Test("""
            a : int = 0
            a = 12 / 2 // '/' > '='
            Print(a)
            """, expectedOutput: "6\n");
        Test("""
            a : int = 0
            a = 12 + 6 / 2 // '/' > '+' > '='
            Print(a)
            """, expectedOutput: "15\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestImplicitDeclare()
    {
        Test("""
            a := 123
            Print(a)
            """, expectedOutput: "123\n");
        Test("""
            var a = 456
            Print(a)
            """, expectedOutput: "456\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestComment()
    {
        Test("""
            var a = 456 /* v1 */ + 789 /* v2 */
            Print(a)
            """, expectedOutput: "1245\n");
        Test("""
            a := /*
                Hi Comment!
                Just wanted to say Hi!
            */ 123
            Print(a)
            """, expectedOutput: "123\n");
        Test("""
            a := 123 /*
                Test End Of Line!
            */ 
            Print(a)
            """, expectedOutput: "123\n");
        Test("""
            a := 123
            
                /*
                    Test Comment in the middle of nowhere
                */ 


            Print(a)
            """, expectedOutput: "123\n");
        Test("""
            a := 123
            if true:

                                                        /*
                    Test Comment in the middle of nowhere
                   */ 


                Print(a)
            """, expectedOutput: "123\n");
        Test("""
            a := 123
            
                    // Test Comment in the middle of nowhere
            
            Print(a)
            """, expectedOutput: "123\n");
        Test("""
            a := 123 // Test Line Comment

                    // And in the middle of nowhere

            Print(a)
            """, expectedOutput: "123\n");
        Test("""
            a := 123
            
            // Test

            // Multiple

            // Line

            // Comment

            Print(a)
            """, expectedOutput: "123\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestCommentWithIndentation()
    {
        Test("""
            a := 123
            if true:
            // in line comment should be able to go anywhere
                Print(a)
            """, expectedOutput: "123\n");
        Test("""
            a := 123
            if true:
                // in line comment should be able to go anywhere
                Print(a)
            """, expectedOutput: "123\n");
        Test("""
            a := 123
            if true:
                    // in line comment should be able to go anywhere
                Print(a)
            """, expectedOutput: "123\n");
        Test("""
            a := 123
            if true:
            /* block comment should be able to go anywhere
                    as long as that line contains no statement */
                Print(a)
            """, expectedOutput: "123\n");
        Test("""
            a := 123
            if true:
                /* block comment should be able to go anywhere
                    as long as that line contains no statement */
                Print(a)
            """, expectedOutput: "123\n");
        Test("""
            a := 123
            if true:
                    /* block comment should be able to go anywhere
                    as long as that line contains no statement */
                Print(a)
            """, expectedOutput: "123\n");
        Test("""
            a := 123
            b := 456
            if false:
                Print(a)
                /* block comments with statement after
                  has the indent equal to the starting
                   point of the block comment */ Print(b)
            """, expectedOutput: "");
        Test("""
            a := 123
            b := 456
            if false:
                Print(a)
            /* block comments with statement after
                has the indent equal to the starting
                point of the block comment
                and no matter how many spaces there are 
                after the end of the
                block comment */          Print(b)
            """, expectedOutput: "456\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestCommentAfterBlock()
    {
        Test("""
            a := 123
            if true:
                Print(a)
            // do not die when there's a comment after a block
            """, expectedOutput: "123\n");
        Test("""
            a := 123
            if true:
                Print(a)
            /* do not die
                when there's a comment
                    after a block */
            """, expectedOutput: "123\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestBlock()
    {
        Test("""
            // do not die when there's a new line after block
            a := 123
            if true:
                Print(a)
            """ + "\n", expectedOutput: "123\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestWhileLoop()
    {
        Test("""
            i := 1
            while i <= 4:
                Print(i)
                i++
            """, expectedOutput: "1\n2\n3\n4\n");
        Test("""
            i := 1
            do:
                Print(i)
                i++
            while i <= 4
            """, expectedOutput: "1\n2\n3\n4\n");
        Test("""
            i := 10
            while i <= 4:
                Print(i)
                i++
            """, expectedOutput: "");
        Test("""
            i := 10
            do:
                Print(i)
                i++
            while i <= 4
            """, expectedOutput: "10\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestNop()
    {
        Test("""
            i := 1
            while i <= 4:
                nop
                Print(i)
                i++
            """, expectedOutput: "1\n2\n3\n4\n");
        Test("""
            nop // do not die
            """, expectedOutput: "");
        // do not die
        Test("", expectedOutput: "");
        Test("""
            // do not die
            """, expectedOutput: "");
        Test("""
            // nop can be used for empty block
            while false:
                nop
            """, expectedOutput: "");
        Test("""
            // nop can be used for empty block
            if true:
                nop
            """, expectedOutput: "");
        Test("""
            // nop can be used for empty block
            if false:
                nop
            else if false:
                nop
            else:
                Print(1)
            """, expectedOutput: "1\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestInline()
    {
        Test("""
            i := 1
            while i <= 4: Print(i++)
            """, expectedOutput: "1\n2\n3\n4\n");
        Test("""
            if true: nop // do not die
            """, expectedOutput: "");
        Test("""
            if false: nop
            else if false: nop
            else: Print(1)
            """, expectedOutput: "1\n");
        Test("""
            i := 1
            while i <= 3:
                if i == 1: Print(0)
                else if i == 2: Print(1)
                else: Print(2)
                i++
            """, expectedOutput: "0\n1\n2\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestForEachLoop()
    {
        //Test("""
        //    for i in 1..5: Print(i)
        //    """, expectedOutput: "1\n2\n3\n4\n");
        //Test("""
        //    // changes to i should not affect the loop
        //    for i in 1..5: Print(++i)
        //    """, expectedOutput: "2\n3\n4\n5\n");
        Test("""
            i := -10 // random value
            for i in 1..5: Print(i)
            Print(i) // 4
            """, expectedOutput: "1\n2\n3\n4\n4\n"); 
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestUnaryWrite()
    {
        Test("""
            i := 1
            Print(i++) // 1
            Print(i) // 2
            """, expectedOutput: "1\n2\n");
        Test("""
            i := 1
            Print(++i) // 2
            Print(i) // 2
            """, expectedOutput: "2\n2\n");
        Test("""
            i := 1
            Print(i--) // 1
            Print(i) // 0
            """, expectedOutput: "1\n0\n");
        Test("""
            i := 1
            Print(--i) // 0
            Print(i) // 0
            """, expectedOutput: "0\n0\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestIf()
    {
        Test("""
            i := 0
            while i <= 10:
                if i % 2 == 0:
                    Print(i)
                i++
            """, expectedOutput: "0\n2\n4\n6\n8\n10\n");
        Test("""
            i := 1
            // prints Abs(i) if i > 0. prints -1 otherwise
            if i > 0:
                Print(i)
            else if i < 0:
                Print(-i)
            else:
                Print(-1)
            """, expectedOutput: "1\n");
        Test("""
            i := -1
            // prints Abs(i) if i > 0. prints -1 otherwise
            if i > 0:
                Print(i)
            else if i < 0:
                Print(-i)
            else:
                Print(-1)
            """, expectedOutput: "1\n");
        Test("""
            i := 0
            // prints Abs(i) if i > 0. prints -1 otherwise
            if i > 0:
                Print(i)
            else if i < 0:
                Print(-i)
            else:
                Print(-1)
            """, expectedOutput: "-1\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestIfNested()
    {
        Test("""
            i := -1
            if i > 0:
                if i >= 10:
                    Print(i + 10)
                else:
                    Print(i)
            // if i <= 0, should print nothing
            """, expectedOutput: "");
        Test("""
            i := 5
            if i > 0:
                if i >= 10:
                    Print(i + 10)
                else:
                    // if 0 < i < 9, should print i
                    Print(i)
            """, expectedOutput: "5\n");
        Test("""
            i := 15
            if i > 0:
                if i >= 10:
                    // if i >= 10, should print i + 10
                    Print(i + 10)
                else:
                    Print(i)
            """, expectedOutput: "25\n");
        Test("""
            i := -1
            if i > 0:
                if i >= 10:
                    Print(i + 10)
            else:
                // if i <= 0, should print i
                Print(i)
            """, expectedOutput: "-1\n");
        Test("""
            i := 1
            if i > 0:
                if i >= 10:
                    Print(i + 10)
                // if 0 < i < 10, should print nothing
            else:
                Print(i)
            """, expectedOutput: "");
        Test("""
            i := 15
            if i > 0:
                if i >= 10:
                    // if i >= 10, should print i + 10
                    Print(i + 10)
            else:
                Print(i)
            """, expectedOutput: "25\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestSimpleFunctionTopLevel()
    {
        Test("""
            // prints x
            func userFunc(x : int):
                Print(x)
            userFunc(123)
            """, expectedOutput: "123\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestFunctionTopLevel()
    {
        Test("""
            i : int = 0

            // prints x + 1
            func userFunc(x : int):
                y := x + 1
                Print(y)
            
            i = 5
            userFunc(i+1) // 7
            userFunc(i+1) // 7
            i = 123
            userFunc(i+1) // 125
            userFunc(i+1) // 125
            """, expectedOutput: "7\n7\n125\n125\n");
        Test("""
            i : int = 0

            // prints i + 1, where inner i != outter i
            func userFunc(i : int):
                i++
                Print(i)
            
            i = 5
            userFunc(i+1) // 7
            userFunc(i+1) // 7
            i = 123
            userFunc(i+1) // 125
            userFunc(i+1) // 125
            """, expectedOutput: "7\n7\n125\n125\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestComplexGotosOnBlock()
    {
        // Tests generated by ChatGPT
        Test("""
            i := 0
            deflb $start
            Print(i) // 0, 1, 2
            i++
            if i < 3:
                goto $start
            Print(10) // 10
            """, expectedOutput: "0\n1\n2\n10\n");
        Test("""
            i := 0
            deflb $outerLoop
            for $innerLoop j in 0..3:
                Print(j) // 0, 1, 2, 0, 1, 2
                if j == 2 and i < 1:
                    i++
                    goto $outerLoop
            """, expectedOutput: "0\n1\n2\n0\n1\n2\n");

        Test("""
            for $loop i in 0..5:
                if i == 3:
                    break $loop
                Print(i) // 0, 1, 2
            Print(10) // 10
            """, expectedOutput: "0\n1\n2\n10\n");
        Test("""
            for $loop i in 0..5:
                if i % 2 == 0:
                    continue $loop
                Print(i) // 1, 3
            """, expectedOutput: "1\n3\n");
        Test("""
            for $outer i in 0..3:
                for $inner j in 0..3:
                    Print(j) // 0, 1, 2
                    if j == 2:
                        exit $outer
                Print(10) // Not printed
            Print(20) // 20
            """, expectedOutput: "0\n1\n2\n20\n");
        Test("""
            func testFunc(i : int) -> int:
                if i % 2 == 0:
                    return i if i > 5
                return -1

            Print(testFunc(4)) // -1
            Print(testFunc(6)) // 6
            Print(testFunc(3)) // -1
            """, expectedOutput: "-1\n6\n-1\n");
        Test("""
            func complexLoop():
                for $loopA i in 1..3:
                    for $loopB j in 0..3:
                        if i == 2 and j == 1:
                            exit $loopA
                        if j == 0:
                            continue $loopB
                        Print(i * 10 + j) // 11, 12
            complexLoop()
            """, expectedOutput: "11\n12\n");
        Test("""
            i := 0
            do $loop:
                Print(i) // 0, 1, 2
                i++
                goto $loop if i < 3 // does not check condition on "while false"
            while false
            Print(10) // 10
            """, expectedOutput: "0\n1\n2\n10\n");
        Test("""
            func earlyExitTest():
                for $outer i in 0..3:
                    for $inner j in 0..3:
                        exit if j == 2
                        Print(i * 10 + j) // 0, 1, 10, 11, 20, 21
            earlyExitTest()
            Print(10) // 10
            """, expectedOutput: "0\n1\n10\n11\n20\n21\n10\n");
        // my test cases
        Test("""
            func abs(i : int) -> int:
                return i if i >= 0
                return -i
            for i in -2..3:
                Print(abs(i))
            """, expectedOutput: "2\n1\n0\n1\n2\n");
        Test("""
            // inspired by GPT test earlier
            for $outer i in 0..3:
                for j in 0..2:
                    continue $outer if i == 1
                    Print(i * 10 + j) // 0, 1, 20, 21
            """, expectedOutput: "0\n1\n20\n21\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestFunctionTopLevelSpilledLocal()
    {
        Test("""
            i : int = 0
            func next:
                i++
            i = 5
            next()
            Print(i) // 6
            next()
            Print(i) // 7
            next()
            Print(i) // 8
            next()
            Print(i) // 9
            """, expectedOutput: "6\n7\n8\n9\n");
        Test("""
            i : int = 5
            Print(i) // 5
            func next:
                i++
            next()
            Print(i) // 6
            next()
            Print(i) // 7
            next()
            Print(i) // 8
            next()
            Print(i) // 9
            """, expectedOutput: "5\n6\n7\n8\n9\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestBackwardGoto()
    {
        Test("""
            func Print1To5:
                i := 1
            $lb1:
                Print(i)
                i++
                if i <= 5:
                    goto $lb1
            Print1To5()
            """, expectedOutput: "1\n2\n3\n4\n5\n");
        Test("""
            func Print1To5:
                i := 1
            $lb1:
                Print(i)
                i++
                goto $lb1 if i <= 5
            Print1To5()
            """, expectedOutput: "1\n2\n3\n4\n5\n");
        Test("""
            i := 1
            deflb $lb1
            Print(i)
            i++
            if i <= 5:
                goto $lb1
            """, expectedOutput: "1\n2\n3\n4\n5\n");
        Test("""
            i := 1
            deflb $lb1
            Print(i)
            i++
            goto $lb1 if i <= 5
            """, expectedOutput: "1\n2\n3\n4\n5\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestBreakContinueExit()
    {
        Test("""
            i := 0
            while i < 10:
                i++
                if i % 2 != 0:
                    continue
                Print(i)
            """, expectedOutput: "2\n4\n6\n8\n10\n");
        Test("""
            i := 0
            while i < 10:
                i++
                continue if i % 2 != 0
                Print(i)
            """, expectedOutput: "2\n4\n6\n8\n10\n");
        Test("""
            for i in 0..100:
                break if i == 7
                Print(i)
            """, expectedOutput: "0\n1\n2\n3\n4\n5\n6\n");
        Test("""
            for i in -2..3:
                if i >= 0:
                    exit if i == 1
                    Print(i) // 0
                else:
                    exit if i == -1
                    Print(i)
                Print(i)
            """, expectedOutput: "-2\n-2\n-1\n0\n0\n1\n2\n2\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestString()
    {
        Test("""
            Print("Some String")
            """, expectedOutput: "Some String\n");
        Test("""
            Print("Hello " + "World")
            """, expectedOutput: "Hello World\n");
        Test("""
            if "Hello World" == "Hello World":
                Print("Pass")
            else:
                Print("Fail")
            """, expectedOutput: "Pass\n");
        Test("""
            Print("Hello World" == "Hello " + "World")
            """, expectedOutput: "True\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestOverload()
    {
        Test("""
            func Overloaded(i : int):
                Print("Int Overload")
            func Overloaded(s : string):
                Print("String Overload")
            
            Overloaded(1)
            Overloaded("Hello")
            Overloaded(2)
            """, expectedOutput: "Int Overload\nString Overload\nInt Overload\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestMethodCall()
    {
        Test("""
            Print("Hello World".Substring(6))
            """, expectedOutput: $"{"Hello World".Substring(6)}\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestStaticCall()
    {
        Test("""
            Print(string.Concat("Hello", "World"))
            """, expectedOutput: $"{string.Concat("Hello", "World")}\n");
    }
    [TestMethod]
    [DoNotParallelize]
    public void TestClass()
    {
        Test("""
            class ABC:
                a : int = 10
                b : string = "Hello"
            
            val := new ABC()
            Print(val.a)
            Print(val.b)
            """, expectedOutput: "10\nHello\n");
    }
    void Test(string topLevelProgram, string? input = null, string? expectedOutput = null)
    {
        AssemblyDefinition ab = AssemblyDefinition.CreateAssembly(new("Test Assembly", new(0,0,0,0)), "<module>", ModuleKind.Dll);
        var entryPoint = QuickCodeCompiler.CompileTopLevelProgramToMSIL(topLevelProgram, ab.MainModule);
        using var ms = new MemoryStream();
        ab.Write(ms);
        ms.Position = 0;

        // Load the assembly dynamically into the current AppDomain
        var loadedAssembly = Assembly.Load(ms.ToArray());

        // Find the generated entry point
        var entryMethod = loadedAssembly.GetModules()[0].GetTypes()[0].GetMethod(entryPoint.Name)!;

        SetupInOut(() =>
        {
            entryMethod.Invoke(null, []);
        }, input, expectedOutput);
    }
    void Test(string topLevelProgram)
    {
        AssemblyDefinition ab = AssemblyDefinition.CreateAssembly(new("Test Assembly", new(0, 0, 0, 0)), "<module>", ModuleKind.Dll);
        var entryPoint = QuickCodeCompiler.CompileTopLevelProgramToMSIL(topLevelProgram, ab.MainModule);
        using var ms = new MemoryStream();
        ab.Write(ms);
        ms.Position = 0;

        // Load the assembly dynamically into the current AppDomain
        var loadedAssembly = Assembly.Load(ms.ToArray());

        // Find the generated entry point
        var entryMethod = loadedAssembly.GetModules()[0].GetTypes()[0].GetMethod(entryPoint.Name)!;

        entryMethod.Invoke(null, []);
    }
    void SetupInOut(Action doThings, string? input = null, string? expectedOutput = null)
    {
        var originalIn = Console.In;
        var originalOut = Console.Out;
        try
        {
            // Redirect Console.ReadLine to a stream containing input
            if (input != null)
            {
                var inputStream = new MemoryStream();
                var writer = new StreamWriter(inputStream);
                writer.Write(input);
                writer.Flush();
                inputStream.Position = 0;
                Console.SetIn(new StreamReader(inputStream));
            }

            // Redirect Console.WriteLine to a stream
            MemoryStream? outputStream = null;
            StreamReader? outputReader = null;
            if (expectedOutput != null)
            {
                outputStream = new MemoryStream();
                var outputWriter = new StreamWriter(outputStream) { AutoFlush = true };
                Console.SetOut(outputWriter);
                outputStream.Position = 0;
                outputReader = new StreamReader(outputStream);
            }

            doThings();

            // Check if the output stream has the same text as the expected output
            if (expectedOutput != null)
            {
                outputStream!.Position = 0;
                var actualOutput = outputReader!.ReadToEnd();
                actualOutput = actualOutput.Replace("\r\n", "\n");
                expectedOutput = expectedOutput.Replace("\r\n", "\n");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(expectedOutput, actualOutput);
            }
        }
        finally
        {
            Console.SetIn(originalIn);
            Console.SetOut(originalOut);
        }
    }
}
