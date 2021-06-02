using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Aga.Controls.Tree
{
	internal class InputWithShift: NormalInputState
	{
        public InputWithShift(TreeViewAdv tree) : base(tree) { }

		protected override void FocusRow(TreeNodeAdv node)
		{
			Tree.SuspendSelectionEvent = true;
			try
			{
				if (Tree.SelectionMode == TreeSelectionMode.Single || Tree.SelectionStart == null)
					base.FocusRow(node);
				else if (CanSelect(node))
				{
					SelectAllFromStart(node);
					Tree.CurrentNode = node;
					Tree.ScrollTo(node);
				}
			}
			finally
			{
				Tree.SuspendSelectionEvent = false;
			}
		}

		protected override void DoMouseOperation(TreeNodeAdvMouseEventArgs args)
		{
			if (Tree.SelectionMode == TreeSelectionMode.Single || Tree.SelectionStart == null)
			{
				base.DoMouseOperation(args);
			}
			else if (CanSelect(args.Node))
			{
				Tree.SuspendSelectionEvent = true;
				try
				{
					SelectAllFromStart(args.Node);
				}
				finally
				{
					Tree.SuspendSelectionEvent = false;
				}
			}
		}

        protected override void MouseDownAtEmptySpace(TreeNodeAdvMouseEventArgs args) { }

		private void SelectAllFromStart(TreeNodeAdv node)
		{
			Tree.ClearSelection();
			int a = node.Row;
			int b = Tree.SelectionStart.Row;

            Enumerable.Range(Math.Min(a, b), Math.Max(a, b)).ToList().ForEach(e => {
                if (Tree.SelectionMode == TreeSelectionMode.Multi || Tree.RowMap[e].Parent == node.Parent)
                    Tree.RowMap[e].IsSelected = true;
            });
		}
	}
}
