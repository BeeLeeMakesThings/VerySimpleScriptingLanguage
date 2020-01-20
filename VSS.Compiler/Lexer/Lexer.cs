using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VSS.Compiler.Lexer
{
    /// <summary>
    /// The state function signature
    /// </summary>
    /// <returns>Should this iteration consume the current character?</returns>
    public delegate bool LexerStateFunction(char currentChar, out LexedToken token);

    /// <summary>
    /// Scans an input stream character-by-character and determines the tokens.
    /// </summary>
    public class Lexer : IDisposable
    {
        private readonly Stream inputStream;
        private readonly bool ownsStream;

        private int lineNumber;
        private int positionNumber;
        private int currentLineNumber;
        private int currentLexedPosition;
        private StringBuilder currentValue;
        private LexerStateFunction StateFunction;

        private bool isEOF;
        private char currentChar;

        // for number lexing
        private bool hasDecimalPlace;
        
        private HashSet<string> keywords;
        private Dictionary<char, char> escapeLookup;

        /// <summary>
        /// Lexes the input stream. The owner of <paramref name="stream"/> is responsible for 
        /// disposing the instance.
        /// </summary>
        public Lexer(Stream stream)
        {
            if (!stream.CanRead)
                throw new ArgumentException("Input stream is not readable.");
            inputStream = stream;
            ownsStream = false;

            CommonStartup();
        }

        /// <summary>
        /// Lexes the input script
        /// </summary>
        public Lexer(string script)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(Encoding.UTF8.GetBytes(script));
            stream.Seek(0, SeekOrigin.Begin);

            ownsStream = true;
            inputStream = stream;

            CommonStartup();
        }

        private void CommonStartup()
        {
            keywords = new HashSet<string>
            {
                "if",
                "while"
            };

            escapeLookup = new Dictionary<char, char>
            {
                { 'n', '\n' },
                { 'r', '\r' },
                { 't', '\t' },
                { '\\', '\\' },
                { '"', '"' }
            };

            lineNumber = currentLineNumber = 1;
            positionNumber = currentLexedPosition = 0; // cause we gonna call NextChar one time first.
            isEOF = false;
            currentValue = new StringBuilder(string.Empty);
            StateFunction = RootState;
            hasDecimalPlace = false;

            // we begin by reading in the first character
            NextChar();
        }

        public void Dispose()
        {
            // if we own the stream, we should clean it up
            if(ownsStream)
            {
                inputStream.Dispose();
            }
        }

        /// <summary>
        /// Consumes the current character and gets the next one.
        /// </summary>
        /// <returns></returns>
        private bool NextChar()
        {
            int readInt = inputStream.ReadByte();
            if (readInt < 0)
            {
                isEOF = true;
                return false;
            }
            else
            {
                currentChar = (char)readInt;
                positionNumber++;
                return true;
            }
        }

        private void ClearState()
        {
            currentValue.Clear();
            StateFunction = RootState;
            hasDecimalPlace = false;
        }

        public LexedToken GetNextToken()
        {
            LexedToken token = null;
            bool consumeChar = false;

            while(!isEOF)
            {
                // line number calculation
                if (currentChar == '\n')
                {
                    lineNumber++;
                    positionNumber = 0;
                }

                consumeChar = StateFunction(currentChar, out token);

                // we consume character only if the last 
                // call asked us to do so
                if (consumeChar) NextChar();

                if (token != null)
                {
                    // we managed to grab a token, so we should clear our state and return 
                    // the token 
                    ClearState();
                    token.PositionNumber = currentLexedPosition;
                    token.LineNumber = currentLineNumber;

                    return token;
                }

                // we didn't get any token
                // perhaps it was a state change
            }
            
            // we reached EOF, we see if we can get anything out of our current state?
            if(currentValue.Length > 0)
            {
                StateFunction('\0', out token);
                if(token!= null)
                {
                    ClearState();
                    token.PositionNumber = currentLexedPosition;
                    token.LineNumber = currentLineNumber;
                }
                return token;
            }
            else
            {
                // nah, nothing left
                return null;
            }
        }

        /// <summary>
        /// The initial state
        /// </summary>
        private bool RootState(char c, out LexedToken token)
        {
            token = null;
            if (char.IsWhiteSpace(c)) // skip whitespaces
                return true; 

            if(char.IsLetter(c) || c == '_') 
            {
                MarkPosition(); 

                // we bumped into a letter
                currentValue.Append(c);

                // so we change to an appropriate state
                StateFunction = LetterState;

                // consume this character
                return true;
            }
            else if(char.IsDigit(c))
            {
                MarkPosition();

                // we got a first digit, let's not consume it first and let the 
                // state function do that
                StateFunction = NumberState;

                return false;
            }
            else if (c == '"')
            {
                MarkPosition();

                // the start of a string
                StateFunction = StringState;

                // consume this character
                return true;
            }
            else if(c == '+')
            {
                token = SingularToken(LexedTokenType.OperatorPlus, c);
                return true;
            }
            else if (c == '-')
            {
                token = SingularToken(LexedTokenType.OperatorMinus, c);
                return true;
            }
            else if (c == '*')
            {
                token = SingularToken(LexedTokenType.OperatorMultiply, c);
                return true;
            }
            else if (c == '/')
            {
                token = SingularToken(LexedTokenType.OperatorDivide, c);
                return true;
            }
            else if (c == '(')
            {
                token = SingularToken(LexedTokenType.OpenParen, c);
                return true;
            }
            else if (c == ')')
            {
                token = SingularToken(LexedTokenType.CloseParen, c);
                return true;
            }
            else if (c == '{')
            {
                token = SingularToken(LexedTokenType.OpenBraces, c);
                return true;
            }
            else if (c == '}')
            {
                token = SingularToken(LexedTokenType.CloseBraces, c);
                return true;
            }
            else if (c == '=')
            {
                MarkPosition();

                // TODO: Transition to a state to get the == 
                currentValue.Append(c);
                token = new LexedToken(LexedTokenType.OperatorEqual, currentValue.ToString());
                return true;
            }

            MarkPosition();
            currentValue.Append(c);
            token = new LexedToken(LexedTokenType.Invalid, currentValue.ToString());
            return true;
        }

        private bool LetterState(char c, out LexedToken token)
        {
            token = null;

            if(char.IsLetterOrDigit(c) || c == '_')
            {
                // bumped into a letter/digit, so we build the string further
                currentValue.Append(c);

                // consume it
                return true;
            }
            else
            {
                // we hit some other random character
                // so we end our processing here and determine what we have
                string value = currentValue.ToString();

                if(keywords.Contains(value))
                {
                    token = new LexedToken(LexedTokenType.Keyword, value);
                }
                else
                {
                    token = new LexedToken(LexedTokenType.Identifier, value);
                }

                // and we don't consume the character in case it is a start of a new token
                return false;
            }
        }

        private bool NumberState(char c, out LexedToken token)
        {
            token = null;

            if(char.IsDigit(c))
            {
                // consume this digit
                currentValue.Append(c);
                return true;
            }
            else if(!hasDecimalPlace && c == '.')
            {
                hasDecimalPlace = true;

                // our first decimal point, consume it
                currentValue.Append(c);
                return true;
            }
            else
            {
                // we end it here since we found some other non-digit
                token = new LexedToken(LexedTokenType.Number, currentValue.ToString());

                // don't consume the character
                return false;
            }
        }

        private bool StringState(char c, out LexedToken token)
        {
            token = null;

            if(c == '\\')
            {
                // we gonna get the escaped character from the next state
                StateFunction = EscapeStringState;

                // consume this slash
                return true;
            }
            else if(c == '"')
            {
                // return the built string
                token = new LexedToken(LexedTokenType.String, currentValue.ToString());

                // consume this closing quotes
                return true;
            }

            // build up the string
            currentValue.Append(c);
            return true;
        }

        private bool EscapeStringState(char c, out LexedToken token)
        {
            token = null;

            if(escapeLookup.ContainsKey(c))
            {
                // we support this escape character
                currentValue.Append(escapeLookup[c]);

                // set the state back to string builder
                StateFunction = StringState;

                // consume this character
                return true;
            }
            else
            {
                // TODO: throw an error possibly
                // right now, we just consume it and do nothing
                StateFunction = StringState;
                return true;
            }
            
        }

        /// <summary>
        /// Creates a token of a single character.
        /// </summary>
        private LexedToken SingularToken(LexedTokenType type, char c)
        {
            MarkPosition();
            currentValue.Append(c);

            return new LexedToken(type, c.ToString());
        }

        private void MarkPosition()
        {
            currentLexedPosition = positionNumber;
            currentLineNumber = lineNumber;
        }
    }
}
