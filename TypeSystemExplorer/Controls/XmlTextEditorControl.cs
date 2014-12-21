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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.TextEditor;
// using ICSharpCode.TextEditor.Addons;
using ICSharpCode.TextEditor.Document;

namespace TypeSystemExplorer.Controls
{
	public class XmlTextEditorControl : TextEditorControl
	{
		public XmlTextEditorControl()
		{
			Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy("XML");
//			Document.FoldingManager.FoldingStrategy = new XmlFoldingStrategy();
			//Document.FormattingStrategy = new XmlFormattingStrategy();
			TextEditorProperties = InitializeProperties();
//			Document.FoldingManager.UpdateFoldings(string.Empty, null);
//			TextArea.Refresh(TextArea.FoldMargin);
		}

		private static ITextEditorProperties InitializeProperties()
		{
			var properties = new DefaultTextEditorProperties();
			properties.Font = new Font("Consolas", 8, FontStyle.Regular);
			properties.IndentStyle = IndentStyle.Smart;
			properties.ShowSpaces = false;
			properties.LineTerminator = Environment.NewLine;
			properties.ShowTabs = false;
			properties.ShowInvalidLines = false;
			properties.ShowEOLMarker = false;
			properties.TabIndent = 2;
			properties.CutCopyWholeLine = true;
			properties.LineViewerStyle = LineViewerStyle.None;
			properties.MouseWheelScrollDown = true;
			properties.MouseWheelTextZoom = true;
			properties.AllowCaretBeyondEOL = false;
			properties.AutoInsertCurlyBracket = true;
			properties.BracketMatchingStyle = BracketMatchingStyle.After;
			properties.ConvertTabsToSpaces = false;
			properties.ShowMatchingBracket = true;
			properties.EnableFolding = true;
			properties.ShowVerticalRuler = false;
			properties.IsIconBarVisible = true;
			properties.Encoding = System.Text.Encoding.Unicode;
//			properties.UseAntiAliasedFont = false;

			return properties;
		}
	}
}
