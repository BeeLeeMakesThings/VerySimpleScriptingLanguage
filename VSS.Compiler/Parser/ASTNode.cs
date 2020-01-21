using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.Compiler.Parser
{
    public class ASTNode
    {
        /// <summary>
        /// List of child nodes. The order in the list matters.
        /// </summary>
        public List<ASTNode> Children { get; }
        
        /// <summary>
        /// The parent of this node. Only the root node has no parent.
        /// </summary>
        public ASTNode Parent { get; }

        public ASTNode(ASTNode parent)
        {
            Children = new List<ASTNode>();
            Parent = parent;
        }
    }
}
