using System;
using VSS.Compiler.Lexer;

namespace VSS
{
    class Program
    {
        static void Main(string[] args)
        {
            Lexer lexer = new Lexer(@"int x 123 123.54 xyz if while whilea _jello = +-*/");

            LexedToken token;
            do
            {
                token = lexer.GetNextToken();
                if (token == null)
                    return;

                Console.WriteLine(token.ToString());
            }
            while (token != null);
        }

        
    }
}
