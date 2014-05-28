using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

using TypeSystemExplorer.Actions;
using TypeSystemExplorer.Models;
using TypeSystemExplorer.Views;

using Clifton.ApplicationStateManagement;
using Clifton.ExtensionMethods;
using Clifton.Tools.Data;
using Clifton.Tools.Strings.Extensions;

using Clifton.SemanticTypeSystem.Interfaces;

namespace TypeSystemExplorer.Controllers
{
	public class VisualizerController : ViewController<VisualizerView>
	{
		public VisualizerController()
		{
		}

		public override void EndInit()
		{
			ApplicationController.VisualizerController = this;
			base.EndInit();
		}

		protected void DragEnterEvent(object sender, DragEventArgs args)
		{
			if (args.Data.GetFormats().Contains("FileDrop"))
			{
				args.Effect = DragDropEffects.Copy;
			}
			else
			{
				args.Effect = DragDropEffects.None;
			}
		}

		protected void DragDropEvent(object sender, DragEventArgs args)
		{
			bool once = true;
			bool receptorsRegistered = false;
			View.DropPoint = new Point(args.X, args.Y);
			View.StartDrop = true;

			if (args.Data.GetFormats().Contains("FileDrop"))
			{
				string[] files = args.Data.GetData("FileDrop") as string[];

				foreach (string fn in files)
				{
					// Attempt to load a receptor.
					if (fn.ToLower().EndsWith(".dll"))
					{
						if (once)
						{
							Say("Loading receptors.");
							once = false;
						}

						Program.Receptors.RegisterReceptor(fn);
						receptorsRegistered = true;
					}
					else if (fn.ToLower().EndsWith(".jpg"))
					{
						if (once)
						{
							Say("Processing files.");
							once = false;
						}

						// Create carriers for each of our images.
						ISemanticTypeStruct protocol = Program.SemanticTypeSystem.GetSemanticTypeStruct("ImageFilename");
						dynamic signal = Program.SemanticTypeSystem.Create("ImageFilename");
						signal.Filename = fn;
						// TODO: The null here is really the "System" receptor.
						Program.Receptors.CreateCarrier(null, protocol, signal);
					}
					else if (fn.ToLower().EndsWith(".xml"))
					{
						// We assume these are carriers we're going to drop.
						if (once)
						{
							Say("Processing carriers.");
							once = false;
						}
						XDocument xdoc = XDocument.Load(fn);

						xdoc.Element("Carriers").Descendants().ForEach(xelem =>
						{
							string protocolName = xelem.Attribute("Protocol").Value;
							ISemanticTypeStruct protocol = Program.Receptors.SemanticTypeSystem.GetSemanticTypeStruct(protocolName);
							dynamic signal = Program.Receptors.SemanticTypeSystem.Create(protocolName);

							// Use reflection to assign all property values since they're defined in the XML.
							xelem.Attributes().Where(a=>a.Name != "Protocol").ForEach(attr =>
							{
								Type t = signal.GetType();
								PropertyInfo pi = t.GetProperty(attr.Name.ToString());
								object val = attr.Value;

								TypeConverter tcFrom = TypeDescriptor.GetConverter(pi.PropertyType);
								//TypeConverter tcTo = TypeDescriptor.GetConverter(typeof(string));

								//if (tcTo.CanConvertTo(t))
								//{
								//	tcTo.ConvertTo(val, pi.PropertyType);
								//}
								
								if (tcFrom.CanConvertFrom(typeof(string)))
								{
									val = tcFrom.ConvertFromInvariantString(attr.Value);
									pi.SetValue(signal, val);
								}
								else
								{
									throw new ApplicationException("Cannot convert string to type " + t.Name);
								}
							});

							Program.Receptors.CreateCarrier(null, protocol, signal);
						});
					}
				}
			}

			if (receptorsRegistered)
			{
				Program.Receptors.LoadReceptors();
			}

			View.StartDrop = false;
		}

		// TODO: Duplicate code.
		protected void Say(string msg)
		{
			ISemanticTypeStruct protocol = Program.SemanticTypeSystem.GetSemanticTypeStruct("TextToSpeech");
			dynamic signal = Program.SemanticTypeSystem.Create("TextToSpeech");
			signal.Text = msg;
			Program.Receptors.CreateCarrier(null, protocol, signal);
		}
	}
}
