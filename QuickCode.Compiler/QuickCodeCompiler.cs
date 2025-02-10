using Get.Lexer;
using Get.Parser;
using Get.PLShared;
using Get.RegexMachine;
using Mono.Cecil;
using QuickCode.Symbols;
using QuickCode.AST.TopLevels;
using QuickCode.MSIL;
using QuickCode.TypeChecking;
using System.Text;

namespace QuickCode.Compiler;

public static class QuickCodeCompiler
{
    static QuickCodeParser Parser = new();
    public static MethodDefinition CompileTopLevelProgramToMSIL(string code, ModuleDefinition assemblyBuilder)
    {
        var lexer = new QuickCodeLexer(new StreamSeeker(new MemoryStream(Encoding.UTF8.GetBytes(code))));
        var tokens =
            lexer.GetTokens()
            .Prepend(new ControlToken(QuickCodeLexer.Tokens.CONTROLTOPLEVELSTATEMENTFILE));
        
        var prog = (TopLevelQuickCodeProgramAST)Parser.Parse(tokens);
        
        var typeCheker = new QuickCodeTypeChecker();
        typeCheker.TypeCheck(prog, QuickCodeDefaultSymbols.Singleton);
        
        var cgen = new QuickCodeMSILGen();
        var mainMethod = cgen.CodeGen(prog, assemblyBuilder);

        return mainMethod;
    }

    record class ControlToken(QuickCodeLexer.Tokens TokenType) : IToken<QuickCodeLexer.Tokens>
    {
        public Position Start => default;

        public Position End => default;
    }

}
