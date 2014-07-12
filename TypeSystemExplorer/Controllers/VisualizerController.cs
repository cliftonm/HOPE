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
using Clifton.Receptor.Interfaces;
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
			View.DropPoint = View.NegativeSurfaceOffsetAdjust(new Point(args.X, args.Y));
			View.StartDrop = true;
			IMembrane dropInto = View.FindInnermostSelectedMembrane(View.DropPoint, Program.Skin);
			dropInto.IfNull(() => dropInto = Program.Skin);
			IReceptor droppedReceptor = null;

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

						droppedReceptor = dropInto.RegisterReceptor(fn);
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
						dropInto.CreateCarrier(Program.Skin["System"].Instance, protocol, signal);
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
						CreateCarriers(dropInto, xdoc.Element("Carriers"));
					}
				}
			}

			if (receptorsRegistered)
			{
				dropInto.LoadReceptors();
				droppedReceptor.Instance.EndSystemInit();
			}

			View.StartDrop = false;
		}

		// TODO: Duplicate code.
		protected void Say(string msg)
		{
			/*
			ISemanticTypeStruct protocol = Program.SemanticTypeSystem.GetSemanticTypeStruct("TextToSpeech");
			dynamic signal = Program.SemanticTypeSystem.Create("TextToSpeech");
			signal.Text = msg;
			// TODO: Is this always the skin membrane?
			Program.Skin.CreateCarrierIfReceiver(null, protocol, signal);
			 */
		}

		public static void CreateCarriers(IMembrane dropInto, XElement el)
		{
			Dictionary<string, ICarrier> carriers = new Dictionary<string, ICarrier>();

			el.Descendants().ForEach(xelem =>
			{
				string protocolName = xelem.Attribute("Protocol").Value;
				ISemanticTypeStruct protocol = Program.Skin.SemanticTypeSystem.GetSemanticTypeStruct(protocolName);
				dynamic signal = Program.Skin.SemanticTypeSystem.Create(protocolName);

				// Use reflection to assign all property values since they're defined in the XML.
				xelem.Attributes().Where(a => a.Name != "Protocol").ForEach(attr =>
				{
					Type t = signal.GetType();
					PropertyInfo pi = t.GetProperty(attr.Name.ToString());

					if (attr.Value.BeginsWith("{"))			// a reference (only references to carriers are supported at the moment)
					{
						ICarrier refCarrier = carriers[attr.Value.Between('{', '}')];
						pi.SetValue(signal, refCarrier);
					}
					else
					{
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
					}
				});

				ICarrier carrier = dropInto.CreateCarrier(Program.Skin["System"].Instance, protocol, signal);
				carriers[protocol.DeclTypeName] = carrier;
			});
		}
	}
}
