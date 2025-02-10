using Get.Lexer;
using Get.PLShared;
using Get.RegexMachine;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace QuickCode;

partial class QuickCodeLexer : LexerBase<QuickCodeLexer.LexerStates, QuickCodeLexer.Tokens>
{
    private partial bool TrueValue() => true;
    private partial bool FalseValue() => false;
    private partial int ParseInt() => int.Parse(MatchedText.Replace("_", ""));
    private partial int ParseHex() => int.Parse(MatchedText.Replace("_", "")[2..], NumberStyles.AllowHexSpecifier);
    private partial int ParseBinary() => int.Parse(MatchedText.Replace("_", "")[2..], NumberStyles.AllowBinarySpecifier);
    private partial string StringUnescape()
    {
        var ros = (ReadOnlySpan<char>)MatchedText;
        ros = ros[1..^1]; // remove first " and last "
        var sb = new StringBuilder(ros.Length);
        var enu = ros.GetEnumerator();
        while (enu.MoveNext())
        {
            if (enu.Current is not '\\')
            {
                sb.Append(enu.Current);
            }
            else
            {
                if (!enu.MoveNext())
                {
                    throw new UnreachableException("Regex should've make sure this");
                }
                sb.Append(EscapeChar(enu.Current));
            }
        }
        return sb.ToString();
    }
    private partial char CharUnescape()
    {
        var ros = (ReadOnlySpan<char>)MatchedText;
        ros = ros[1..^1]; // remove first ' and last '
        if (ros[0] is not '\\')
        {
            if (ros.Length is not 1)
                throw new UnreachableException("Regex should've make sure this");
            return ros[0];
        }
        else
        {
            if (ros.Length is not 2)
                throw new UnreachableException("Regex should've make sure this");
            return EscapeChar(ros[1]);
        }
    }
    static char EscapeChar(char charAfterSlash)
        => charAfterSlash switch
        {
            'n' => '\n',
            'r' => '\r',
            't' => '\t',
            '\'' => '\'',
            '\"' => '\"',
            _ => throw new UnreachableException("Regex should've make sure this")
        };
}