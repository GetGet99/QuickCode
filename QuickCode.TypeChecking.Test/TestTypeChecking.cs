using Get.Lexer;
using Get.PLShared;
using QuickCode.AST.FileProgram;
using QuickCode.Compiler;
using QuickCode.Symbols;
using QuickCode.Symbols.Compiler.Implementation;
using System.Text;

namespace QuickCode.TypeChecking.Test
{
    [TestClass]
    public sealed class TestTypeChecking
    {
        [TestMethod]
        public void TestAnalysis()
        {
            var (prog, gs) = CompileFile("""
                namespace NS:
                    class SampleClass:
                        i : int = 0
                        j : int = 1
                        func PrintMe:
                            Print(this.i)
                            Print(this.j)
                """);
            if (gs["NS"] is not NamespaceSymbol ns)
            {
                Assert.Fail();
                return;
            }
            if (ns["SampleClass"] is not ITypeSymbol sampleClass)
            {
                Assert.Fail();
                return;
            }
            if (sampleClass.Children["i"] is not FieldSymbol i)
            {
                Assert.Fail();
                return;
            }
            Assert.AreEqual((ITypeSymbol)ITypeSymbol.Int32, (ITypeSymbol)i.Type);
            if (sampleClass.Children["j"] is not FieldSymbol j)
            {
                Assert.Fail();
                return;
            }
            Assert.AreEqual((ITypeSymbol)ITypeSymbol.Int32, (ITypeSymbol)j.Type);
            if (sampleClass.Children["PrintMe"] is not QuickCodeFuncSymbol printme)
            {
                Assert.Fail();
                return;
            }
            Assert.AreEqual(1, printme.Parameters.Length);
            Assert.AreEqual((sampleClass, "this"), printme.Parameters[0]);
        }
        readonly static QuickCodeParser Parser = new();
        (QuickCodeFileProgramAST program, GlobalSymbols symbols) CompileFile(string program)
        {
            var lexer = new QuickCodeLexer(new StreamSeeker(new MemoryStream(Encoding.UTF8.GetBytes(program))));
            var tokens =
                lexer.GetTokens()
                .Prepend(new ControlToken(QuickCodeLexer.Tokens.CONTROLNORMALFILE));

            var prog = (QuickCodeFileProgramAST)Parser.Parse(tokens);
            GlobalSymbols gs = new();
            QuickCodeAnalyzer.Analyze(
                toplevel: null,
                [prog],
                gs,
                QuickCodeDefaultSymbols.Singleton
            );
            return (prog, gs);
        }
        record class ControlToken(QuickCodeLexer.Tokens TokenType) : IToken<QuickCodeLexer.Tokens>
        {
            public Position Start => default;

            public Position End => default;
        }
    }
}
