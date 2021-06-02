using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Aga.Controls.Tree.NodeControls;

namespace Aga.Controls.Tree
{
	public struct DrawContext
	{
		public Graphics Graphics { get; set; }
		public Rectangle Bounds { get; set; }
		public Font Font { get; set; }
		public DrawSelectionMode DrawSelection { get; set; }
		public bool DrawFocus { get; set; }
		public NodeControl CurrentEditorOwner { get; set; }
		public bool Enabled { get; set; }
	}
}
