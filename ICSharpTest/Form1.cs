using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpTest
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			TextEditorControl ctrl = new TextEditorControl();
			ctrl.Dock = DockStyle.Fill;
			ctrl.Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy("XML");
			ctrl.TextEditorProperties = InitializeProperties();
			// ctrl.Document.FormattingStrategy = new Xml

			Controls.Add(ctrl);
		}

		private static ITextEditorProperties InitializeProperties()
		{
			var properties = new DefaultTextEditorProperties();
			properties.Font = new Font("Consolas", 9, FontStyle.Regular);
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
