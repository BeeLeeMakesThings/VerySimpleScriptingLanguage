using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.Compiler.Lexer
{
    public enum LexedTokenType
    {
        Identifier,
        OperatorPlus,
        OperatorMinus,
        OperatorMultiply,
        OperatorDivide,
        OperatorEqual,

        Number,

        Keyword,

        OpenBraces,
        CloseBraces,

        OpenParen,
        CloseParen,
        
        String,

        Semicolon,

        Invalid
    }
}
