using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Clifton.SemanticTypeSystem;
using Clifton.SemanticTypeSystem.Interfaces;

using TypeSystemExplorer.Controls;
using TypeSystemExplorer.Models;

using Clifton.ExtensionMethods;
using Clifton.Tools.Data;
using Clifton.Windows.Forms;
using Clifton.Windows.Forms.XmlTree;

namespace TypeSystemExplorer.Views
{
	public class SemanticTypeEditorView : PaneView
	{
		public delegate void NotificationDlgt();

		public event NotificationDlgt Opening;
		public event NotificationDlgt Closing;

		public ApplicationModel Model { get; protected set; }
		public XTree TreeView { get; protected set; }
		public override string MenuName { get { return "mnuSemanticTypeEditor"; } }

		public override void EndInit()
		{
			Opening.IfNotNull().Then(() => Opening());
			base.EndInit();
		}

		public void Clear()
		{
			TreeView.Clear();
		}

		public void Update(STS sts)
		{
			// TODO: Implement with the new TV schema stuff.
//			Clear();
//			PopulateTree(sts);
		}

		protected override void WhenHandleDestroyed(object sender, EventArgs e)
		{
			Closing.IfNotNull().Then(() => Closing());
			base.WhenHandleDestroyed(sender, e);
		}

		public TreeNode AddNode(IXtreeNode inst, TreeNode parent)
		{
			TreeNode node = TreeView.AddNode(inst, parent);

			return node;
		}
/*
		protected void PopulateTree(STS sts)
		{
			sts.SemanticTypes.ForEach(t =>
			{
				object tn = TreeView.AddNode(null, t.Key, t);
				PopulateSTChildren(t.Value, tn);
			});
		}

		protected void PopulateSTChildren(ISemanticType st, object parent)
		{
			TreeView.AddNode(parent, "decl", st.Decl);
			TreeView.AddNode(parent, "struct", st.Struct);

			st.Struct.SemanticElements.ForEach(elem =>
			{
				object tn = TreeView.AddNode(parent, elem.Name, elem);
				PopulateSTChildren(elem.Element, tn);
			});
		}
*/ 
	}
}
