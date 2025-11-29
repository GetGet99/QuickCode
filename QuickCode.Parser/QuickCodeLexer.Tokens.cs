using Get.LangSupport;
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
        [TextmateCommentScope(Regexes = [@"//[^\r\n]*"], Priority = (int)TextmateOrder.LineComment)]
        [TextmateCommentScope(Begin = @"/\*", End = @"/\*", Priority = (int)TextmateOrder.BlockComment)]
        Comment,


        [Regex<IdentifierAST>(@"[a-zA-Z][a-zA-Z0-9]*", nameof(CreateIdentifier), State = (int)LexerStates.Code)]
        [TextmateOtherVariableScope(VariableType.Other, Priority = (int)TextmateOrder.Identifier)]
        Identifier,
        [Regex<IdentifierAST>(@"list", nameof(CreateIdentifier2), State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Other, Priority = (int)TextmateOrder.Keywords)]
        List,
        [Regex<IdentifierAST>(@"array", nameof(CreateIdentifier3), State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Other, Priority = (int)TextmateOrder.Keywords)]
        Array,

        [Regex(@"deflb", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Control, Priority = (int)TextmateOrder.Keywords)]
        DefLabel,
        [Regex(@"if", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Control, Priority = (int)TextmateOrder.Keywords)]
        If,
        [Regex(@"else", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Control, Priority = (int)TextmateOrder.Keywords)]
        Else,
        [Regex(@"while", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Control, Priority = (int)TextmateOrder.Keywords)]
        While,
        [Regex(@"do", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Control, Priority = (int)TextmateOrder.Keywords)]
        Do,
        [Regex(@"goto", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Control, Priority = (int)TextmateOrder.Keywords)]
        Goto,
        [Regex(@"func", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Declaration, Priority = (int)TextmateOrder.Keywords)]
        Func,
        [Regex(@"return", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Control, Priority = (int)TextmateOrder.Keywords)]
        Return,
        [Regex(@"nop", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Control, Priority = (int)TextmateOrder.Keywords)]
        Nop,
        [Regex(@"var", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Declaration, Priority = (int)TextmateOrder.Keywords)]
        Var,
        [Regex(@"not", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Other, Priority = (int)TextmateOrder.Keywords)]
        Not,
        [Regex(@"for", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Control, Priority = (int)TextmateOrder.Keywords)]
        For,
        [Regex(@"new", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Other, Priority = (int)TextmateOrder.Keywords)]
        New,
        [Regex(@"in", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Other, Priority = (int)TextmateOrder.Keywords)]
        In,
        [Regex(@"namespace", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Declaration, Priority = (int)TextmateOrder.Keywords)]
        Namespace,
        [Regex(@"break", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Control, Priority = (int)TextmateOrder.Keywords)]
        Break,
        [Regex(@"continue", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Control, Priority = (int)TextmateOrder.Keywords)]
        Continue,
        [Regex(@"exit", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Control, Priority = (int)TextmateOrder.Keywords)]
        Exit,
        [Regex(@"and", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Other, Priority = (int)TextmateOrder.Keywords)]
        And,
        [Regex(@"or", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Other, Priority = (int)TextmateOrder.Keywords)]
        Or,
        [Regex(@"class", State = (int)LexerStates.Code, Order = (int)Order.KeywordAndSpecialSyntax)]
        [TextmateKeywordScope(KeywordType.Declaration, Priority = (int)TextmateOrder.Keywords)]
        Class,

        [Regex(@"\$", State = (int)LexerStates.Code)]
        [TextmateConstantScope(ConstantType.Other, Regexes = [@"\$[a-zA-Z][a-zA-Z0-9]*"], Priority = (int)TextmateOrder.SpecialIdentifier)]
        DollarSign,
        [Regex(@":", State = (int)LexerStates.Code)]
        [TextmatePunctuationSeparatorScope(PunctuationSeparatorType.Colon, Priority = (int)TextmateOrder.OperatorsAndPunctuations)]
        Colon,
        [Regex(@"\.", State = (int)LexerStates.Code)]
        [TextmatePunctuationSeparatorScope(PunctuationSeparatorType.Dot, Priority = (int)TextmateOrder.OperatorsAndPunctuations)]
        Dot,
        [Regex(@",", State = (int)LexerStates.Code)]
        [TextmatePunctuationSeparatorScope(PunctuationSeparatorType.Comma, Priority = (int)TextmateOrder.OperatorsAndPunctuations)]
        Comma,
        [Regex(@"->", State = (int)LexerStates.Code)]
        [TextmateKeywordScope(KeywordType.Other, Priority = (int)TextmateOrder.OperatorsAndPunctuations)]
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
        [TextmateConstantLanguageScope(ConstantLanguageType.Boolean, Priority = (int)TextmateOrder.Keywords)]
        Boolean,
        [Regex<int>(@"(-|)[0-9][0-9_]*", nameof(ParseInt), State = (int)LexerStates.Code)]
        [TextmateConstantNumericScope(NumericType.Decimal, Priority = (int)TextmateOrder.Number, Regexes = [@"(-|)[0-9][0-9_]*"])]
        [Regex<int>(@"0x[0-9a-fA-F]+", nameof(ParseHex), State = (int)LexerStates.Code)]
        [TextmateConstantNumericScope(NumericType.Hex, Priority = (int)TextmateOrder.Number, Regexes = [@"0x[0-9a-fA-F]+"])]
        [Regex<int>(@"0b[01]+", nameof(ParseBinary), State = (int)LexerStates.Code)]
        [TextmateConstantNumericScope(NumericType.Binary, Priority = (int)TextmateOrder.Number, Regexes = [@"0b[01]+"])]
        Integer,
        [Regex<string>("""
            "([^\r\n\"\\]|(\\(n|t|r|\'|\")))*"
            """, nameof(StringUnescape), State = (int)LexerStates.Code)]
        [TextmateStringQuotedScope(StringQuotedType.Double, Priority = (int)TextmateOrder.StringChar)]
        String,
        [Regex<char>("""
            '([^\r\n\'\\]|(\\(n|t|r|\'|\")))'
            """, nameof(CharUnescape), State = (int)LexerStates.Code)]
        [TextmateStringQuotedScope(StringQuotedType.Single, Priority = (int)TextmateOrder.StringChar)]
        Char,

        [Regex(@"\+\+", State = (int)LexerStates.Code)]
        [TextmateKeywordOperatorScope(OperatorType.Increment)]
        Increment,
        [Regex(@"\-\-", State = (int)LexerStates.Code)]
        [TextmateKeywordOperatorScope(OperatorType.Decrement)]
        Decrement,
        [Regex(@"\+", State = (int)LexerStates.Code)]
        [TextmateKeywordOperatorScope(OperatorType.Arithmetic)]
        Plus,
        [Regex(@"\-", State = (int)LexerStates.Code)]
        [TextmateKeywordOperatorScope(OperatorType.Arithmetic)]
        Minus,
        [Regex(@"\*", State = (int)LexerStates.Code)]
        [TextmateKeywordOperatorScope(OperatorType.Arithmetic)]
        Multiply,
        [Regex(@"/", State = (int)LexerStates.Code)]
        [TextmateKeywordOperatorScope(OperatorType.Arithmetic)]
        Divide,
        [Regex(@"%", State = (int)LexerStates.Code)]
        [TextmateKeywordOperatorScope(OperatorType.Arithmetic)]
        Modulo,
        [Regex(@"<", State = (int)LexerStates.Code)]
        [TextmateKeywordOperatorScope(OperatorType.Arithmetic)]
        LessThan,
        [Regex(@">", State = (int)LexerStates.Code)]
        [TextmateKeywordOperatorScope(OperatorType.Arithmetic)]
        MoreThan,
        [Regex(@"<=", State = (int)LexerStates.Code)]
        [TextmateKeywordOperatorScope(OperatorType.Arithmetic)]
        LessThanOrEqual,
        [Regex(@">=", State = (int)LexerStates.Code)]
        [TextmateKeywordOperatorScope(OperatorType.Arithmetic)]
        MoreThanOrEqual,
        [Regex(@"==", State = (int)LexerStates.Code)]
        [TextmateKeywordOperatorScope(OperatorType.Arithmetic)]
        Equal,
        [Regex(@"!=", State = (int)LexerStates.Code)]
        [TextmateKeywordOperatorScope(OperatorType.Arithmetic)]
        NotEqual,
        [Regex(@"=", State = (int)LexerStates.Code)]
        [TextmateKeywordOperatorScope(OperatorType.Assignment)]
        Assign,
        [Regex(@":=", State = (int)LexerStates.Code)]
        [TextmateKeywordOperatorScope(OperatorType.Assignment)]
        Declare,
        [Regex(@"\.\.", State = (int)LexerStates.Code)]
        [TextmateKeywordOperatorScope(OperatorType.Arithmetic)]
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
    enum TextmateOrder : int
    {
        Regular = 0,
        Identifier = 0,
        SpecialIdentifier = 1,
        Number = 2,
        OperatorsAndPunctuations = 2,
        Keywords = 3,
        StringChar = 4,
        LineComment = 5,
        BlockComment = 6
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