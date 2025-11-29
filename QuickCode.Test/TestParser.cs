using Get.Lexer;
using Get.PLShared;
using Get.RegexMachine;
using QuickCode.AST.Classes;
using QuickCode.AST.TopLevels;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static QuickCode.QuickCodeLexer;
using static QuickCode.QuickCodeLexer.Tokens;
namespace QuickCode.Test;

static class TestParser
{
    public static StreamSeeker StreamOf(string text) => new(new MemoryStream(Encoding.UTF8.GetBytes(text)));
    static void AssertNoMoreToken(IEnumerator<IToken<Tokens>> tokens)
    {
        if (tokens.MoveNext())
            Debugger.Break();
    }
    static void AssertNextTokenIs(IEnumerator<IToken<Tokens>> tokens, Tokens tokenType)
    {
        if (!tokens.MoveNext() || tokens.Current.TokenType != tokenType)
            Debugger.Break();
    }
    static void AssertNextTokenIs<T>(IEnumerator<IToken<Tokens>> tokens, Tokens tokenType, T value) where T : notnull
    {
        if (!tokens.MoveNext() ||
            tokens.Current.TokenType != tokenType ||
            tokens.Current is not IToken<Tokens, T> type ||
            !type.Data.Equals(value))
            Debugger.Break();
    }
    public static void Test()
    {
        QuickCodeLexer Lexer = new(StreamOf(
            """
            class ABC:
                a : int = 10
                b : string = "Hello"

            val := new ABC()
            val.first.second
            """
        ));
        var tokens = Lexer.GetTokens().Prepend(new ControlToken(CONTROLTOPLEVELSTATEMENTFILE));
        QuickCodeParser parser = new();
        var start = parser.Parse(tokens);
        if (start.AssertIs<TopLevelQuickCodeProgramAST>(out var casted))
        {
            if (casted.TopLevelProgramComponentASTs[0].AssertIs<QuickCodeClassAST>(out var cls))
            {
                //cls.Declarables[0].AssertIs(out var )
            }
        }
    }
    record class ControlToken(QuickCodeLexer.Tokens TokenType) : IToken<QuickCodeLexer.Tokens>
    {
        public Position Start => default;

        public Position End => default;
    }
    static bool AssertIs<T>(this object value, [NotNullWhen(true)] out T? casted)
    {
        if (value is T val)
        {
            casted = val;
            return true;
        }
        Debugger.Break();
        casted = default;
        return false;
    }

}
