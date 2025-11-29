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
using QuickCode.Compiler.Symbols;
using QuickCode.Symbols.Compiler.Implementation;

namespace QuickCode.Compiler;

public static class QuickCodeCompiler
{
    static QuickCodeParser Parser = new();
    public static MethodDefinition CompileTopLevelProgramToMSIL(string code, ModuleDefinition module)
    {
        var lexer = new QuickCodeLexer(new StreamSeeker(new MemoryStream(Encoding.UTF8.GetBytes(code))));
        var tokens =
            lexer.GetTokens()
            .Prepend(new ControlToken(QuickCodeLexer.Tokens.CONTROLTOPLEVELSTATEMENTFILE));
        
        var prog = (TopLevelQuickCodeProgramAST)Parser.Parse(tokens);
        
        var typeCheker = new QuickCodeTypeChecker();
        var typeFactory = new MSILTypeFactory(module);
        typeCheker.TypeCheck(prog, typeFactory, new GlobalSymbols(), new QuickCodeDefaultSymbols(typeFactory));
        
        var cgen = new QuickCodeMSILGen();
        var mainMethod = cgen.CodeGen(typeFactory, prog, module);

        return mainMethod;
    }

    record class ControlToken(QuickCodeLexer.Tokens TokenType) : IToken<QuickCodeLexer.Tokens>
    {
        public Position Start => default;

        public Position End => default;
    }

}
