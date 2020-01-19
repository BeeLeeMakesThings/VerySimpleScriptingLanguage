using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.Compiler.Lexer
{
    /// <summary>
    /// Represents a general lexeme. 
    /// </summary>
    public class LexedToken
    {
        /// <summary>
        /// Type of lexeme
        /// </summary>
        public LexedTokenType Type { get; }

        /// <summary>
        /// The raw string value of this lexeme
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// The line number of this token
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// The position along the line of this token
        /// </summary>
        public int PositionNumber { get; set; }

        public LexedToken(LexedTokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return $"({Type}, {Value})";
        }
    }
}
