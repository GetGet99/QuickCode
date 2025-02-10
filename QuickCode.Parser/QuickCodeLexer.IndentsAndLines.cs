using Get.PLShared;
using Get.RegexMachine;

namespace QuickCode;

partial class QuickCodeLexer
{
    ITextSeekable TextStream { get; } = text;
    Stack<uint> prevIndentCounts = new([0]);
    private partial void IndentCallback()
    {
        if (HasReachedEOF)
        {
            // EOF
            // last new line is already emitted
            EmitAllDedentsAndEnd();
            return;
        }
        IndentProcess(MatchedText);
    }
    private void EmitAllDedentsAndEnd()
    {
        // emit all dedents
        while (prevIndentCounts.Count > 1)
        {
            prevIndentCounts.Pop();
            YieldToken(Tokens.Dedent);
        }
        End();
    }
    void IndentProcess(ReadOnlySpan<char> matchedText)
    {
        uint indentCount = 0;

        // count indent
        foreach (char c in matchedText)
        {
            if (c is ' ') indentCount++;
            if (c is '\t') indentCount += 4; // tabs are 4 spaces, idc
        }
        var curIndent = prevIndentCounts.Peek();
        if (indentCount > curIndent)
        {
            prevIndentCounts.Push(indentCount);
            YieldToken(Tokens.Indent);
        }
        if (indentCount < curIndent)
        {
            while (prevIndentCounts.Count > 1)
            {
                var prevIndent = prevIndentCounts.Peek();
                if (indentCount < prevIndent)
                {
                    prevIndentCounts.Pop();
                    YieldToken(Tokens.Dedent);
                }
                else if (indentCount > prevIndent)
                {
                    // technically its an indentation error, but let parser handle it
                    // parser will not expect an indent here
                    YieldToken(Tokens.Indent);
                }
                else
                {
                    // if equal, break
                    break;
                }
            }
        }
        // if equal, emit nothing
        // start looking for code
        GoTo(LexerStates.Code);
    }
    private partial void IndentNewLineDiscardCallback() { }
    private partial void IndentLineCommentDiscardCallback() { }
    private partial void IndentBlockCommentDiscardCallback() { }
    private partial void IndentBlockCommentWithoutNewLineCallback() {
        if (HasReachedEOF)
        {
            // last new line has already been emitted.
            EmitAllDedentsAndEnd();
        } else
        {
            // there's something after.
            // we get the indents before the comment
            // then use that as the indentation of this statement
            ReadOnlySpan<char> matchedText = MatchedText;
            // get the text before the block comment
            // '/' character is guaranteed to exist so we can just
            matchedText = matchedText[..matchedText.IndexOf('/')];
            IndentProcess(matchedText);
        }
    }
    //private partial string ReturnMatchedText() => MatchedText;
    private partial IToken<Tokens> Newline()
    {
        // indentation check
        GoTo(LexerStates.Indentation);
        return Make(Tokens.Newline);
    }
    private partial IToken<Tokens> NewLogicalLine()
    {
        // logical line check: do not allow two logical lines
        GoTo(LexerStates.NewLogicalLine);
        return Make(Tokens.Newline);
    }
    private partial void UnknownHandler()
    {
        if (TextStream.MoveNext())
            YieldToken(Make(Tokens.Unknown, TextStream.Current));
        else
        {
            // EOF
            // emit last new line
            YieldToken(Tokens.Newline);
            // emit dedents
            EmitAllDedentsAndEnd();
        }
    }
    private partial void LogicalLineUnknownHandler()
    {
        if (!HasReachedEOF)
        {
            // Next up: it's the code
            GoTo(LexerStates.Code);
        }
        else
        {
            // EOF
            // last new line is already emitted
            // emit dedents
            EmitAllDedentsAndEnd();
        }
    }
    private partial void NewLineAfterNewLogicalLine()
        // just act like there is no semicolon
        => GoTo(LexerStates.Indentation);
}