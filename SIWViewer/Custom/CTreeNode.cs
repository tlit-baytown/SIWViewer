using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SIWViewer.Custom
{
    public class CTreeNode : TreeNode
    {
        /// <summary>
        /// Gets or sets the type of the tree node. The type is an integer value with a default of 1. Another known value is 2.
        /// </summary>
        [DefaultValue(1)]
        public int TypeNode { get; set; }

        public CTreeNode(string name, int type) : base(name)
        {
            TypeNode = type;
        }
    }
}
