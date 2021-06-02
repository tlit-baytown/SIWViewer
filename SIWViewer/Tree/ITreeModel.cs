using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Aga.Controls.Tree
{
	public interface ITreeModel
	{
        /// <summary>
        /// Get the children of the specified <see cref="TreePath"/>.
        /// </summary>
        /// <param name="treePath">The path in the tree to get the children of.</param>
        /// <returns>An <see cref="IEnumerable"/> of children.</returns>
		IEnumerable GetChildren(TreePath treePath);

        /// <summary>
        /// Get a value indicating if the specified <see cref="TreePath"/> is considered a leaf node.
        /// </summary>
        /// <param name="treePath">The path in the tree to check.</param>
        /// <returns>True if the path is a leaf node; False otherwise.</returns>
		bool IsLeaf(TreePath treePath);

        /// <summary>
        /// Occurs when TreeNodes are changed.
        /// </summary>
		event EventHandler<TreeModelEventArgs> NodesChanged;
        /// <summary>
        /// Occurs when TreeNodes are inserted.
        /// </summary>
		event EventHandler<TreeModelEventArgs> NodesInserted;
        /// <summary>
        /// Occurs when TreeNodes are removed.
        /// </summary>
		event EventHandler<TreeModelEventArgs> NodesRemoved;
        /// <summary>
        /// Occurs when the TreeNode structure is changed.
        /// </summary>
		event EventHandler<TreePathEventArgs> StructureChanged;
	}
}
