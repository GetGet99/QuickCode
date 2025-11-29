using Get.Lexer;
using Get.PLShared;
using Get.RegexMachine;
using QuickCode.AST.Expressions;
using System.Diagnostics;
using System.Text;
using static QuickCode.QuickCodeLexer;
using static QuickCode.QuickCodeLexer.Tokens;
namespace QuickCode.Test;

static class TestLexer
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
            func fname:
                print(123)
                print(0x1234)
                print(0b1010)
                print(true)
                print(false)
                print("Hello String \n\r\t")
                return true
            """
        ));
        var tokens = Lexer.GetTokens().GetEnumerator();
        static void PrintCheck<T>(IEnumerator<IToken<Tokens>> tokens, Tokens paramToken, T value) where T : notnull
        {
            AssertNextTokenIs(tokens, Identifier, new IdentifierAST("print"));
            AssertNextTokenIs(tokens, OpenBracket);
            AssertNextTokenIs(tokens, paramToken, value);
            AssertNextTokenIs(tokens, CloseBracket);
            AssertNextTokenIs(tokens, Newline);
        }
        AssertNextTokenIs(tokens, Func);
        AssertNextTokenIs(tokens, Identifier, new IdentifierAST("fname"));
        AssertNextTokenIs(tokens, Colon);
        AssertNextTokenIs(tokens, Newline);
        AssertNextTokenIs(tokens, Indent);
        PrintCheck(tokens, Integer, 123);
        PrintCheck(tokens, Integer, 0x1234);
        PrintCheck(tokens, Integer, 0b1010);
        PrintCheck(tokens, Tokens.Boolean, true);
        PrintCheck(tokens, Tokens.Boolean, false);
        PrintCheck(tokens, Tokens.String, "Hello String \n\r\t");
        AssertNextTokenIs(tokens, Return);
        AssertNextTokenIs(tokens, Tokens.Boolean, true);
        AssertNextTokenIs(tokens, Newline);
        AssertNextTokenIs(tokens, Dedent);
        AssertNoMoreToken(tokens);
    }
}
