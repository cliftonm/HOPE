/*
    Copyright 2104 Higher Order Programming

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.ExtensionMethods;

namespace TypeSystemExplorer.Controls
{
	public class TreeViewControl : TreeView
	{
		public void Clear()
		{
			Nodes.Clear();
		}

		public object AddNode(object parent, string name, object tag)
		{
			TreeNode node = null;

			if (parent == null)
			{
				node = Nodes.Add(name);
				node.Tag = tag;
			}
			else
			{
				node = ((TreeNode)parent).Nodes.Add(name);
				node.Tag = tag;
			}

			return node;
		}
	}
}
