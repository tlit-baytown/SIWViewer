using System;
using System.Collections.Generic;
using System.Text;

namespace Aga.Controls.Tree
{
	public interface IToolTipProvider
	{
        /// <summary>
        /// Get the tool tip of the specified <see cref="TreeNodeAdv"/> control.
        /// </summary>
        /// <param name="node"></param>
        /// <returns>A string tooltip.</returns>
		string GetToolTip(TreeNodeAdv node);
	}
}
