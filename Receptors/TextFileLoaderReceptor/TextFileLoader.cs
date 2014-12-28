using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

using Clifton.MycroParser;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace TextFileLoaderReceptor
{
    public class TextFileLoader : BaseReceptor
    {
		public override string Name { get { return "Text File Loader"; } }
		public override string ConfigurationUI { get { return "TextFileLoaderConfig.xml"; } }

		[UserConfigurableProperty("Filename:")]
		public string FileName { get; set; }

		[MycroParserInitialize("tbFileName")]
		protected TextBox tbFileName;

		public TextFileLoader(IReceptorSystem rsys)
			: base(rsys)
		{
			AddEmitProtocol("Text");
		}

		protected void ShowFileChooser(object sender, EventArgs args)
		{
			FileDialog ofd = new OpenFileDialog();
			ofd.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";

			if (ofd.ShowDialog() == DialogResult.OK)
			{
				tbFileName.Text = ofd.FileName;
			}
		}

		public override bool UserConfigurationUpdated()
		{
			base.UserConfigurationUpdated();
			bool ret = LoadAndSendFileText(FileName);

			return ret;
		}

		public override void EndSystemInit()
		{
			base.EndSystemInit();
			LoadAndSendFileText(FileName);
		}

		protected bool LoadAndSendFileText(string filename)
		{
			Subname = Path.GetFileName(filename);
			bool ret = false;

			try
			{
				string text = File.ReadAllText(filename);
				SendTextSignal(text);
				ret = true;
			}
			catch (Exception ex)
			{
				ConfigurationError = ex.Message;
			}

			return ret;
		}

		public void SendTextSignal(string text)
		{
			CreateCarrier("Text", signal => signal.Value = text);
		}
    }
}

