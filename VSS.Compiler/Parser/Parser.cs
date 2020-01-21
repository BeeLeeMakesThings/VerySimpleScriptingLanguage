using System;
using System.Collections.Generic;
using System.Text;
using VSS.Compiler.Lexer;

namespace VSS.Compiler.Parser
{
    /// <summary>
    /// Takes in a sequence of tokens and creates an AST
    /// </summary>
    public class Parser
    {
        private readonly Stack<ParserState> stack;
        private readonly IEnumerable<LexedToken> tokens;

        public Parser(IEnumerable<LexedToken> tokens)
        {
            this.tokens = tokens;

            stack = new Stack<ParserState>();
        }

        public ASTNode BuildAST()
        {

        }
    }
}
