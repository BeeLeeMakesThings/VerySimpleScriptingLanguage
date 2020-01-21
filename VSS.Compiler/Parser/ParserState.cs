using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.Compiler.Parser
{
    public delegate void ParserStateFunction();

    public class ParserState
    {
        public ParserStateFunction State { get; set; }

        public ASTNode Node { get; set; }
    }
}
