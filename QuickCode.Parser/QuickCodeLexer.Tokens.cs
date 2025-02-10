using Get.Lexer;
using Get.RegexMachine;
using QuickCode.AST.Expressions;

namespace QuickCode;
[Lexer<Tokens>]
public partial class QuickCodeLexer(ITextSeekable text) : LexerBase<QuickCodeLexer.LexerStates, QuickCodeLexer.Tokens>(text, LexerStates.Indentation)
{
    [CompileTimeConflictCheck]
    public enum Tokens
    {
        // Indents and new lines
        [Regex(@"[\t ]*", nameof(IndentCallback), ShouldReturnToken = false, State = (int)LexerStates.Indentation)]
        [Regex(@"[\t \r\n]*[\r\n]", nameof(IndentNewLineDiscardCallback), ShouldReturnToken = false, State = (int)LexerStates.Indentation)]
        // does not need a new line (case of EOF).
        // even if it has one, it will be matched with the
        // discard new line rule
        [Regex(@"[\t \r\n]*//[^\r\n]*", nameof(IndentLineCommentDiscardCallback), ShouldReturnToken = false, State = (int)LexerStates.Indentation)]
        [Regex(@"[\t \r\n]*/\*[^\*]*\*+([^/\*][^\*]*\*+)*/[\r\n]", nameof(IndentBlockCommentDiscardCallback), ShouldReturnToken = false, State = (int)LexerStates.Indentation)]
        // EOF case means this rule is needed
        [Regex(@"[\t \r\n]*/\*[^\*]*\*+([^/\*][^\*]*\*+)*/", nameof(IndentBlockCommentWithoutNewLineCallback), ShouldReturnToken = false, State = (int)LexerStates.Indentation)]
        Indent, Dedent,
        [Regex(@"([\r\n \t]|(/\*[^\*]*\*+([^/\*][^\*]*\*+)*/)|(//[^\r\n]*))*[\r\n]", nameof(QuickCodeLexer.Newline), State = (int)LexerStates.Code)]
        [Regex(@";", nameof(NewLogicalLine), State = (int)LexerStates.Code)]
        [Regex(@"[\r\n]", nameof(NewLineAfterNewLogicalLine), ShouldReturnToken = false, State = (int)LexerStates.NewLogicalLine)]
        Newline,
        // don't output anything against whitespace
        [Regex(@"[ \t]*", ShouldReturnToken = false, State = (int)LexerStates.NewLogicalLine)]
        // + cuz it will not invoke the empty rule
        [Regex(@"[ \t]+", ShouldReturnToken = false, State = (int)LexerStates.Code)]
        Whitespace,
        // @"" matches EOF or unknown
        [Regex(@"", nameof(UnknownHandler), ShouldReturnToken = false, State = (int)LexerStates.Code, Order = (int)Order.Fallback)]
        [Regex(@"", nameof(LogicalLineUnknownHandler), ShouldReturnToken = false, State = (int)LexerStates.NewLogicalLine, Order = (int)Order.Fallback)]
        Unknown,
        // matches the line comment till the end of line. Then the new line rule later can handle it
        [Regex(@"//[^\r\n]*", ShouldReturnToken = false, State = (int)LexerStates.Code, Order = (int)Order.Comment)]
        //[Regex(@"//[^\r\n]*", ShouldReturnToken = false, State = (int)LexerStates.NewLogicalLine, Order = (int)Order.Comment)]
        // reference: https://stackoverflow.com/questions/13014947/regex-to-match-a-c-style-multiline-comment
        [Regex(@"/\*[^\*]*\*+([^/\*][^\*]*\*+)*/", ShouldReturnToken = false, State = (int)LexerStates.Code, Order = (int)Order.Comment)]
        //[Regex(@"/\*[^\*]*\*+([^/\*][^\*]*\*+)*/", ShouldReturnToken = false, State = (int)LexerStates.NewLogicalLine, Order = (int)Order.Comment)]
        Comment,


        [Regex<IdentifierAST>(@"[a-zA-Z][a-zA-Z0-9]*", nameof(CreateIdentifier), State = (int)LexerStates.Code)]
        Identifier,
        [Regex<IdentifierAST>(@"list", nameof(CreateIdentifier2), State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        List,
        [Regex<IdentifierAST>(@"array", nameof(CreateIdentifier3), State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        Array,

        [Regex(@"deflb", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        DefLabel,
        [Regex(@"if", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        If,
        [Regex(@"else", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        Else,
        [Regex(@"while", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        While,
        [Regex(@"do", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        Do,
        [Regex(@"goto", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        Goto,
        [Regex(@"func", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        Func,
        [Regex(@"return", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        Return,
        [Regex(@"nop", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        Nop,
        [Regex(@"var", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        Var,
        [Regex(@"not", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        Not,
        [Regex(@"for", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        For,
        [Regex(@"in", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        In,
        [Regex(@"break", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        Break,
        [Regex(@"continue", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        Continue,
        [Regex(@"exit", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        Exit,
        [Regex(@"and", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        And,
        [Regex(@"or", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        Or,
        [Regex(@"class", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        Class,

        [Regex(@"\$", State = (int)LexerStates.Code)]
        DollarSign,
        [Regex(@":", State = (int)LexerStates.Code)]
        Colon,
        [Regex(@",", State = (int)LexerStates.Code)]
        Comma,
        [Regex(@"->", State = (int)LexerStates.Code)]
        Arrow,
        [Regex(@"\(", State = (int)LexerStates.Code)]
        OpenBracket,
        [Regex(@"\)", State = (int)LexerStates.Code)]
        CloseBracket,
        [Regex(@"\[", State = (int)LexerStates.Code)]
        OpenSquareBracket,
        [Regex(@"\]", State = (int)LexerStates.Code)]
        CloseSquareBracket,


        [Regex<bool>(@"true", nameof(TrueValue), State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [Regex<bool>(@"false", nameof(FalseValue), State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        Boolean,
        [Regex<int>(@"(-|)[0-9][0-9_]*", nameof(ParseInt), State = (int)LexerStates.Code)]
        [Regex<int>(@"0x[0-9a-fA-F]+", nameof(ParseHex), State = (int)LexerStates.Code)]
        [Regex<int>(@"0b[01]+", nameof(ParseBinary), State = (int)LexerStates.Code)]
        Integer,
        [Regex<string>("""
            "([^\r\n\"\\]|(\\(n|t|r|\'|\")))*"
            """, nameof(StringUnescape), State = (int)LexerStates.Code)]
        String,
        [Regex<char>("""
            '([^\r\n\'\\]|(\\(n|t|r|\'|\")))'
            """, nameof(CharUnescape), State = (int)LexerStates.Code)]
        Char,

        [Regex(@"\+\+", State = (int)LexerStates.Code)]
        Increment,
        [Regex(@"\-\-", State = (int)LexerStates.Code)]
        Decrement,
        [Regex(@"\+", State = (int)LexerStates.Code)]
        Plus,
        [Regex(@"\-", State = (int)LexerStates.Code)]
        Minus,
        [Regex(@"\*", State = (int)LexerStates.Code)]
        Multiply,
        [Regex(@"/", State = (int)LexerStates.Code)]
        Divide,
        [Regex(@"%", State = (int)LexerStates.Code)]
        Modulo,
        [Regex(@"<", State = (int)LexerStates.Code)]
        LessThan,
        [Regex(@">", State = (int)LexerStates.Code)]
        MoreThan,
        [Regex(@"<=", State = (int)LexerStates.Code)]
        LessThanOrEqual,
        [Regex(@">=", State = (int)LexerStates.Code)]
        MoreThanOrEqual,
        [Regex(@"==", State = (int)LexerStates.Code)]
        Equal,
        [Regex(@"!=", State = (int)LexerStates.Code)]
        NotEqual,
        [Regex(@"=", State = (int)LexerStates.Code)]
        Assign,
        [Regex(@":=", State = (int)LexerStates.Code)]
        Declare,
        [Regex(@"\.\.", State = (int)LexerStates.Code)]
        Range,
        CONTROLTOPLEVELSTATEMENTFILE,
        CONTROLNORMALFILE,

        PostfixIncDecPrecedence,
        PrefixIncDecPrecedence,
    }
    
    private partial IdentifierAST CreateIdentifier()
    {
        return new IdentifierAST(MatchedText);
    }
    private partial IdentifierAST CreateIdentifier2() => CreateIdentifier();
    private partial IdentifierAST CreateIdentifier3() => CreateIdentifier();
    enum Order : int
    {
        Fallback = -1,
        Initial = 0,
        KeywordAndSpecialSyntax = 1,
        Comment = 2
    }
    public enum LexerStates
    {
        Indentation,
        Initial = Indentation,
        Code,
        NewLogicalLine,
        LineComment
    }
}