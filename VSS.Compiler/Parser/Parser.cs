using System;
using System.Collections.Generic;
using System.Text;
using VSS.Compiler.Lexer;
using VSS.Compiler.Parser.Nodes;

namespace VSS.Compiler.Parser
{
    /// <summary>
    /// Takes in a sequence of tokens and creates an AST
    /// </summary>
    public class Parser
    {
        private readonly Stack<ParserState> stack;
        private readonly IEnumerator<LexedToken> tokens;

        private LexedToken currentToken;

        public Parser(IEnumerable<LexedToken> tokens)
        {
            this.tokens = tokens.GetEnumerator();
            currentToken = null;

            stack = new Stack<ParserState>();
        }

        public ASTNode BuildAST()
        {
            ASTNode root = new RootNode();

            // create a root state to start the parser
            ParserState rootState = new ParserState(root, RootState);
            ParserState state;

            stack.Push(rootState);

            // we get the first token to kick things off
            NextToken();

            while(currentToken != null)
            {
                // peek the top of the stack to see what state we are at
                state = stack.Peek();

                state.State();
            }

            // Verify that there's only the root state left
            state = stack.Pop();
            if (state != rootState)
                throw new Exception(); // TODO: Throw a better error

            return root;
        }

        /// <summary>
        /// Advances the enumerator.
        /// </summary>
        /// <returns>False if there are no more tokens</returns>
        private bool NextToken()
        {
            bool result = tokens.MoveNext();
            currentToken = tokens.Current;
            return result;
        }

        /// <summary>
        /// Validates that the current token type is as specified. Then moves the enumerator forward.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>False if the current token is not expected.</returns>
        private bool ConsumeToken(LexedTokenType type)
        {
            if (currentToken == null)
                return false;
            if (currentToken.Type != type)
                return false;

            NextToken();
            return true;
        }

        private bool CurrentTokenIs(LexedTokenType type)
        {
            return currentToken != null && currentToken.Type == type;
        }

        /////////////////// STATE FUNCTIONS ///////////////////

        private void RootState()
        {
            
        }

    }
}
