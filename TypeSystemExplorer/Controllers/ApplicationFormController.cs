using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Windows.Forms;

using WeifenLuo.WinFormsUI.Docking;

using Clifton.ApplicationStateManagement;
using Clifton.Assertions;
using Clifton.ExtensionMethods;
using Clifton.MycroParser;
using Clifton.Receptor;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Strings;
using Clifton.Tools.Strings.Extensions;

using TypeSystemExplorer.Actions;
using TypeSystemExplorer.Models;
using TypeSystemExplorer.Views;

namespace TypeSystemExplorer.Controllers
{
	public class ApplicationFormController : ViewController<ApplicationFormView>, IReceptorInstance
	{
		public IMruMenu MruMenu { get; protected set; }

		public string CurrentFilename
		{
			get { return ApplicationModel.Filename; }
			protected set
			{
				ApplicationModel.Filename = value;
			}
		}

		public SemanticTypeTreeController SemanticTypeTreeController { get; protected set; }
		public PropertyGridController PropertyGridController { get; protected set; }
		public XmlEditorController XmlEditorController { get; set; }		// The active editor.
		public OutputController OutputController { get; set; }
		public SymbolTableController SymbolTableController { get; set; }
		public VisualizerController VisualizerController { get; set; }

		protected Applet applet;

		public ApplicationFormController()
		{

			// documentControllerMap = new DiagnosticDictionary<IDockContent, NotecardController>("DocumentControllerMap");
			RegisterUserStateOperations();
			Program.Skin.RegisterReceptor("System", this);
			Program.Skin.NewMembrane += OnNewMembrane;
		}

		/// <summary>
		/// Register ourselves ("System") as a receptor and listen to new membranes being added to this membrane.
		/// </summary>
		protected void OnNewMembrane(object sender, MembraneEventArgs e)
		{
			e.Membrane.RegisterReceptor("System", this);
			e.Membrane.NewMembrane += OnNewMembrane;
			e.Membrane.LoadReceptors();				// finish initializing the system receptor.
		}

		protected void FormClosingEvent(object sender, FormClosingEventArgs args)
		{
			InternalReset();
			VisualizerController.View.Stop();
			args.Cancel = false;
		}

		protected void RegisterUserStateOperations()
		{
			Program.AppState.Register("Form", () =>
			{
				return new List<State>()
						{
							new State("X", View.Location.X),
							new State("Y", View.Location.Y),
							new State("W", View.Size.Width),
							new State("H", View.Size.Height),
							new State("WindowState", View.WindowState.ToString()),
							new State("Last Opened", CurrentFilename),
						};

			},
				state =>
				{
					// Silently handle exceptions for when we add state items that are part of the state file until we 
					// save the state.  This allows us to add new state information without crashing the app on startup.
					Assert.SilentTry(() => View.Location = new Point(state.Single(t => t.Key == "X").Value.to_i(), state.Single(t => t.Key == "Y").Value.to_i()));
					Assert.SilentTry(() => View.Size = new Size(state.Single(t => t.Key == "W").Value.to_i(), state.Single(t => t.Key == "H").Value.to_i()));
					Assert.SilentTry(() => View.WindowState = state.Single(t => t.Key == "WindowState").Value.ToEnum<FormWindowState>());
					Assert.SilentTry(() => CurrentFilename = state.Single(t => t.Key == "Last Opened").Value);
				});
		}

		public override void EndInit()
		{
			Assert.SilentTry(() => Program.AppState.RestoreState("Form"));
		}

		public void LoadXml(string filename)
		{
			XmlEditorController.IfNull(() => NewDocument("xmlEditor.xml"));
			XmlEditorController.View.Editor.LoadFile(filename);
			CurrentFilename = filename;
			SetCaption(filename);

			CreateTypes(this, EventArgs.Empty);
			GenerateCode(this, EventArgs.Empty);
			Compile(this, EventArgs.Empty);

			// Now we can load our receptors, once the protocol dictionary is loaded.
			// TODO: How do we KNOW the protocol dictionary has been loaded?
			// Speak("Protocols loaded.");
			Program.Skin.LoadReceptors();				// Process immediately.
		}

		protected void ActiveDocumentChanged(object sender, EventArgs args)
		{
		}

		protected void ContentRemoved(object sender, DockContentEventArgs e)
		{
		}

		/// <summary>
		/// Resets the STS but keeps the XML document text.
		/// </summary>
		protected void Reset(object sender, EventArgs args)
		{
			// Speak("System reset.");
			SemanticTypeTreeController.IfNotNull(c => c.View.Clear());
			PropertyGridController.IfNotNull(c => c.View.Clear());
			// XmlEditorController.IfNotNull(c => c.View.Clear());
			OutputController.IfNotNull(c => c.View.Clear());
			SymbolTableController.IfNotNull(c => c.View.Clear());
			InternalReset();
			Program.Skin.RegisterReceptor("System", this);
		}

		protected void InternalReset()
		{
			Program.SemanticTypeSystem.Reset();
			Program.Skin.Reset();
			VisualizerController.View.Reset();
		}

		protected void About(object sender, EventArgs args)
		{
			Form form = MycroParser.InstantiateFromFile<Form>("about.xml", null);
			form.ShowDialog();
		}

		protected void NewXml(object sender, EventArgs args)
		{
			if (CheckDirtyModel())
			{
				XmlEditorController.IfNotNull(t => t.View.Editor.Document.TextContent = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n");
				CurrentFilename = String.Empty;
			}
		}

		protected void OpenXml(object sender, EventArgs args)
		{
			if (CheckDirtyModel())
			{
				OpenFileDialog ofd = new OpenFileDialog();
				ofd.RestoreDirectory = true;
				ofd.CheckFileExists = true;
				ofd.Filter = "XML (.xml)|*.xml";
				ofd.Title = "Load XML";
				DialogResult res = ofd.ShowDialog();

				if (res == DialogResult.OK)
				{
					// MruMenu.AddFile(ofd.FileName);
					LoadXml(ofd.FileName);
					CurrentFilename = ofd.FileName;
				}
			}
		}

		protected void SaveXml(object sender, EventArgs args)
		{
			if (!ApplicationModel.HasFilename)
			{
				SaveXmlAs(sender, args);
			}
			else
			{
				XmlEditorController.View.Editor.SaveFile(CurrentFilename);
			}
		}

		protected void SaveXmlAs(object sender, EventArgs args)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.RestoreDirectory = true;
			sfd.CheckPathExists = true;
			sfd.Filter = "XML (.xml)|*.xml";
			sfd.Title = "Save XML";
			DialogResult res = sfd.ShowDialog();

			if (res == DialogResult.OK)
			{
				CurrentFilename = sfd.FileName;
				XmlEditorController.View.Editor.SaveFile(sfd.FileName);
				// MruMenu.AddFile(sfd.FileName);
				// SetCaption(sfd.FileName);
			}
		}

		protected void Exit(object sender, EventArgs args)
		{
			CheckDirtyModel().Then(() => View.Close());
		}

		protected void Closing(object sender, CancelEventArgs args)
		{
			CheckDirtyModel().Then(() =>
			{
				SaveLayout();
				Program.AppState.SaveState("Form");
			}).Else(() => args.Cancel = true);
		}

		/// <summary>
		/// The first time the form is displayed, try loading the last opened deck.
		/// </summary>
		protected void Shown(object sender, EventArgs args)
		{
			// Because I get tired of doing this manually.
			LoadXml("protocols.xml");
			// LoadApplet();
		}

		/// <summary>
		/// When the application is activated (selected), load the last document and set the focus to the browser or HTML editor.
		/// </summary>
		protected void Activated(object sender, EventArgs args)
		{
			// ActiveDocumentController.IfNotNull(f => f.SetDocumentFocus());
		}

		/// <summary>
		/// Sets the caption to the filename.
		/// </summary>
		protected void SetCaption(string filename)
		{
			CurrentFilename = filename;
			View.SetCaption(filename);
		}

		protected void LoadTheLayout(string layoutFilename)
		{
			View.DockPanel.LoadFromXml(layoutFilename, ((string persistString) =>
			{
				string typeName = persistString.LeftOf(',').Trim();
				string contentMetadata = persistString.RightOf(',').Trim();
				IDockContent container = InstantiateContainer(typeName, contentMetadata);
				InstantiateContent(container, contentMetadata);

				return container;
			}));
		}

		protected void LoadLayout(object sender, EventArgs args)
		{
			if (File.Exists("layout.xml"))
			{
				LoadTheLayout("layout.xml");
			}
			else
			{
				RestoreLayout(sender, args);
			}
		}

		protected void RestoreLayout(object sender, EventArgs args)
		{
			CloseAllDockContent();
			LoadTheLayout("defaultLayout.xml");
		}

		protected void SaveLayout()
		{
			// Close documents first, so we don't get dummy documents when we reload the layout.
			CloseAllDocuments();
			View.DockPanel.SaveAsXml("layout.xml");
		}

		protected IDockContent InstantiateContainer(string typeName, string metadata)
		{
			IDockContent container = null;

			if (typeName == typeof(GenericPane).ToString())
			{
				container = new GenericPane(metadata);
			}
			else if (typeName == typeof(GenericDocument).ToString())
			{
				container = new GenericDocument(metadata);
			}

			return container;
		}

		protected void InstantiateContent(object container, string filename)
		{
			MycroParser.InstantiateFromFile<object>(filename, ((MycroParser mp) =>
			{
				mp.AddInstance("Container", container);
				mp.AddInstance("ApplicationFormController", this);
				mp.AddInstance("ApplicationModel", ApplicationModel);
			}));
		}

		protected void CloseAllDockContent()
		{
			View.CloseAll();
		}

		protected void CloseAllDocuments()
		{
			View.CloseDocuments();
		}

		public bool CheckDirtyModel()
		{
			return true;
		}

		protected void NewDocument(string filename)
		{
			GenericDocument doc = new GenericDocument(filename);
			InstantiateContent(doc, filename);
			doc.Show(View.DockPanel);
		}

		protected void NewPane(string filename)
		{
			GenericPane pane = new GenericPane(filename);
			InstantiateContent(pane, filename);
			pane.Show(View.DockPanel);
		}

		protected void ShowSemanticTypeTree(object sender, EventArgs args)
		{
			SemanticTypeTreeController.IfNull(() =>
			{
				NewPane("semanticTypeTree.xml");
			});
		}

		protected void ShowXmlEditor(object sender, EventArgs args)
		{
			XmlEditorController.IfNull(() =>
			{
				NewDocument("xmlEditor.xml");
			});
		}

		protected void ShowSymbolTable(object sender, EventArgs args)
		{
			SymbolTableController.IfNull(() =>
			{
				NewDocument("symbolTable.xml");
			});
		}

		protected void ShowVisualizer(object sender, EventArgs args)
		{
			VisualizerController.IfNull(() =>
			{
				NewDocument("visualizer.xml");
			});
		}

		protected void ShowPropertyGrid(object sender, EventArgs args)
		{
			PropertyGridController.IfNull(() =>
			{
				NewDocument("propertyGrid.xml");
			});
		}

		protected void ShowOutput(object sender, EventArgs args)
		{
			OutputController.IfNull(() =>
				{
					NewDocument("output.xml");
				});
		}

		public void SetMenuCheckedState(string menuName, bool state)
		{
			View.SetMenuCheckState(menuName, state);
		}

		public void SetMenuEnabledState(string menuName, bool state)
		{
			View.SetMenuEnabledState(menuName, state);
		}

		public void PaneClosed(PaneView pane)
		{
			if (pane is SemanticTypeTreeView)
			{
				SemanticTypeTreeController = null;
			}
			else if (pane is PropertyGridView)
			{
				PropertyGridController = null;
			}
			else
			{
				throw new ApplicationException("Unknown pane : " + pane.GetType().FullName);
			}
		}

		public void CreateTypes(object sender, EventArgs args)
		{
			try
			{
				Program.SemanticTypeSystem.Parse(XmlEditorController.View.Editor.Document.TextContent);

				if (SemanticTypeTreeController != null)
				{
					SemanticTypeTreeController.View.Update(Program.SemanticTypeSystem);
				}
			}
			catch (Exception ex)
			{
				LogException(ex);
			}
		}

		public void GenerateCode(object sender, EventArgs args)
		{
			if (Program.SemanticTypeSystem.SemanticTypes.Count == 0)
			{
				CreateTypes(sender, args);
			}

			string result = Program.SemanticTypeSystem.GenerateCode();
			OutputController.View.Editor.Document.TextContent = result;
		}

		public void Compile(object sender, EventArgs args)
		{
			if (Program.SemanticTypeSystem.SemanticTypes.Count == 0)
			{
				CreateTypes(sender, args);
			}

			string result = Program.SemanticTypeSystem.GenerateCode();
			OutputController.View.Editor.Document.TextContent = result;

			try
			{
				Assert.ClearErrorMessage();
				System.Reflection.Assembly assy = Compiler.Compile(result);
				Program.SemanticTypeSystem.CompiledAssembly = assy;
			}
			catch (Exception ex)
			{
				LogException(ex);
			}
			/*
						dynamic t = Program.SemanticTypeSystem.Create("myShortLine");
						t.Line.PointA.Point.X.Integer.Value = 1;
						t.Line.PointA.Point.Y.Integer.Value = 2;
						t.Line.PointB.Point.X.Integer.Value = 12;
						t.Line.PointB.Point.Y.Integer.Value = 13;
						SymbolTableController.View.UpdateSymbolTable(Program.SemanticTypeSystem.SymbolTable);
			*/
			// heehee.  It works!
			// dynamic t = Program.SemanticTypeSystem.Create("Point");
			// t.X.Integer.Value = 5;
		}

		public void LoadReceptors(object sender, EventArgs args)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
			DialogResult ret = ofd.ShowDialog();

			if (ret == DialogResult.OK)
			{
				LoadApplet(ofd.FileName);
				MruMenu.AddFile(ofd.FileName);
			}
		}

		public void SaveReceptors(object sender, EventArgs args)
		{
			SaveReceptorsInternal(CurrentFilename);
		}

		public void SaveReceptorsAs(object sender, EventArgs args)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
			sfd.OverwritePrompt = true;
			DialogResult ret = sfd.ShowDialog();

			if (ret == DialogResult.OK)
			{
				SaveReceptorsInternal(sfd.FileName);
				MruMenu.AddFile(sfd.FileName);
			}
		}

		public void SaveReceptorsInternal(string filename)
		{
			CurrentFilename = filename;
			SetCaption(filename);

			XmlDocument xdoc = new XmlDocument();
			
			//Add the namespaces used in books.xml to the XmlNamespaceManager.
			// xmlnsManager.AddNamespace("ixm", "TypeSystemExplorer.Models, TypeSystemExplorer");

			XmlNode mycroXaml = xdoc.CreateElement("MycroXaml");
			AddAttribute(mycroXaml, "xmlns:ixm", "TypeSystemExplorer.Models, TypeSystemExplorer");
			AddAttribute(mycroXaml, "xmlns:def", "def");
			AddAttribute(mycroXaml, "xmlns:ref", "ref");
			xdoc.AppendChild(mycroXaml);
			AddAttribute(mycroXaml, "Name", "Form");
			
			XmlNode appletNode = xdoc.CreateElement("ixm", "Applet", "TypeSystemExplorer.Models, TypeSystemExplorer");
			mycroXaml.AppendChild(appletNode);
			
			XmlNode membranesDefNode = xdoc.CreateElement("ixm", "MembranesDef", "TypeSystemExplorer.Models, TypeSystemExplorer");
			appletNode.AppendChild(membranesDefNode);

			// Save membrane and its receptors.
			SerializeMembrane(membranesDefNode, Program.Skin);
			
			// Save the carriers defined in the applet that was loaded.
			if (applet != null)
			{
				if (appletNode != null)
				{
					XmlNode carriersDef = xdoc.CreateElement("ixm", "CarriersDef", "TypeSystemExplorer.Models, TypeSystemExplorer");
					appletNode.AppendChild(carriersDef);
					XmlNode carriers = xdoc.CreateElement("ixm", "Carriers", "TypeSystemExplorer.Models, TypeSystemExplorer");
					carriersDef.AppendChild(carriers);

					// Were there any carriers defined in the original?
					// TODO: See TODO Comment in ReceptorDef.cs model and MycroParser bug.
					if (applet.CarriersDef != null)
					{
						applet.CarriersDef.Carriers.ForEach(c =>
							{
								XmlNode carrierDef = xdoc.CreateElement("ixm", "CarrierDef", "TypeSystemExplorer.Models, TypeSystemExplorer");
								AddAttribute(carrierDef, "Protocol", c.Protocol);
								carriers.AppendChild(carrierDef);
								XmlNode attr = xdoc.CreateElement("ixm", "Attributes", "TypeSystemExplorer.Models, TypeSystemExplorer");
								carrierDef.AppendChild(attr);

								c.Attributes.ForEach(a =>
									{
										XmlNode attrVal = xdoc.CreateElement("ixm", "Attr", "TypeSystemExplorer.Models, TypeSystemExplorer");
										AddAttribute(attrVal, "Name", a.Name);
										AddAttribute(attrVal, "Value", a.Value);
										attr.AppendChild(attrVal);
									});
							});
					}
				}
			}

			xdoc.Save(filename);
		}

		protected void SerializeMembrane(XmlNode membranesDefNode, IMembrane m)
		{
			XmlDocument xdoc = membranesDefNode.OwnerDocument;

			XmlNode membranesNode = xdoc.CreateElement("ixm", "Membranes", "TypeSystemExplorer.Models, TypeSystemExplorer");
			membranesDefNode.AppendChild(membranesNode);

			XmlNode membraneDefNode = xdoc.CreateElement("ixm", "MembraneDef", "TypeSystemExplorer.Models, TypeSystemExplorer");
			AddAttribute(membraneDefNode, "Name", m.Name);
			membranesNode.AppendChild(membraneDefNode);

			// Serialize receptors.

			XmlNode receptors = xdoc.CreateElement("ixm", "Receptors", "TypeSystemExplorer.Models, TypeSystemExplorer");
			membraneDefNode.AppendChild(receptors);

			// TODO: The serialization needs to start at the membrane level and support recursion into inner membranes.
			m.Receptors.ForEach(r =>
			{
				// Ignore internal receptors that register themselves.
				if (!String.IsNullOrEmpty(r.AssemblyName))
				{
					XmlNode rNode = xdoc.CreateElement("ixm", "ReceptorDef", "TypeSystemExplorer.Models, TypeSystemExplorer");
					receptors.AppendChild(rNode);
					AddAttribute(rNode, "Name", r.Name);
					AddAttribute(rNode, "AssemblyName", r.AssemblyName);
					AddAttribute(rNode, "Enabled", r.Enabled.ToString());

					if (!r.Instance.IsHidden)
					{
						Point p = VisualizerController.View.GetLocation(r);
						AddAttribute(rNode, "Location", p.X + ", " + p.Y);
					}
				}
			});

			// Serialize membranes.

			XmlNode permeabilities = xdoc.CreateElement("ixm", "Permeabilities", "TypeSystemExplorer.Models, TypeSystemExplorer");
			membraneDefNode.AppendChild(permeabilities);

			m.ProtocolPermeability.ForEach(kvp =>
				{
					XmlNode permeable = xdoc.CreateElement("ixm", "PermeabilityDef", "TypeSystemExplorer.Models, TypeSystemExplorer");
					AddAttribute(permeable, "Protocol", kvp.Key);
					AddAttribute(permeable, "Direction", kvp.Value.Direction.ToString());
					AddAttribute(permeable, "Permeable", kvp.Value.Permeable.ToString());
					permeabilities.AppendChild(permeable);
				});

			// Recurse into child membranes (if they have receptors.)
			m.Membranes.ForEach(innerMembrane =>
				{
					if (innerMembrane.Receptors.Count > 0)
					{
						SerializeMembrane(membraneDefNode, innerMembrane);
					}
				});
			
		}

		public void LoadApplet(string filename)
		{
			CurrentFilename = filename;
			SetCaption(filename);
			applet = MycroParser.InstantiateFromFile<Applet>(filename, null);

			// Create the membranes and their containing receptors:
			List<Membrane> membraneList = new List<Membrane>();
			membraneList.Add(Program.Skin);

			VisualizerController.View.StartDrop = true;
			VisualizerController.View.ShowMembranes = false;
			// Skin is the the root membrane.  It has no siblings.
			DeserializeMembranes(applet.MembranesDef.Membranes[0], Program.Skin, membraneList);
			VisualizerController.View.StartDrop = false;
			VisualizerController.View.ShowMembranes = true;

			// Create the carriers if they exist.

			if (applet.CarriersDef != null)
			{
				applet.CarriersDef.Carriers.ForEach(c =>
					{
						ISemanticTypeStruct protocol = Program.Skin.SemanticTypeSystem.GetSemanticTypeStruct(c.Protocol);
						dynamic signal = Program.Skin.SemanticTypeSystem.Create(c.Protocol);
						Type t = signal.GetType();

						c.Attributes.ForEach(attr =>
							{
								PropertyInfo pi = t.GetProperty(attr.Name);
								TypeConverter tcFrom = TypeDescriptor.GetConverter(pi.PropertyType);

								if (tcFrom.CanConvertFrom(typeof(string)))
								{
									object val = tcFrom.ConvertFromInvariantString(attr.Value);
									pi.SetValue(signal, val);
								}
								else
								{
									throw new ApplicationException("Cannot convert string to type " + t.Name);
								}
							});

						// TODO: Carriers need to specify into which membrane they are placed.
						Membrane inMembrane = membraneList.SingleOrDefault(m => m.Name == c.Membrane);

						if (inMembrane != null)
						{
							inMembrane.CreateCarrier(Program.Skin["System"].Instance, protocol, signal);
						}
						else
						{
							// TODO: Inform user that carrier is not associated with a defined membrane.
						}
					});
			}

			// When we're all done, recreate the connections because the membrane permeability might have changed.
			// TODO: Should this be an event the visualizer picks up on?
			VisualizerController.View.UpdateConnections();
		}

		protected void DeserializeMembranes(MembraneDef membraneDef, Membrane membrane, List<Membrane> membraneList)
		{
			Dictionary<IReceptor, Point> receptorLocationMap = new Dictionary<IReceptor, Point>();
			Point noLocation = new Point(-1, -1);
			membrane.Name = membraneDef.Name;

			membraneDef.Receptors.ForEach(n =>
				{
					IReceptor r = membrane.RegisterReceptor(n.Name, n.AssemblyName);
					receptorLocationMap[r] = n.Location;
					r.Enabled = n.Enabled;
				});

			// After registration, but before the NewReceptor fire event, set the drop point.
			// Load all the receptors defined in this membrane first.
			membrane.LoadReceptors((rec) =>
			{
				Point p;

				// Internal receptors, like ourselves, will not be in this deserialized collection.
				if (receptorLocationMap.TryGetValue(rec, out p))
				{
					if (p == noLocation)
					{
						VisualizerController.View.ClientDropPoint = VisualizerController.View.GetRandomLocation();
					}
					else
					{
						VisualizerController.View.ClientDropPoint = p;
					}
				}
			});

			// Load the permeability configuration.  We do this after loading the receptors, because
			// this way the permeability has been initialized via the NewReceptor event hook.
			membraneDef.Permeabilities.ForEach(p =>
				{
					if (!membrane.ProtocolPermeability.ContainsKey(p.Protocol))
					{
						membrane.ProtocolPermeability[p.Protocol] = new PermeabilityConfiguration() { Direction = p.Direction };
					}

					membrane.ProtocolPermeability[p.Protocol].Permeable = p.Permeable;
				});

			// Next, load the inner membrane and receptors.
			membraneDef.Membranes.ForEach(innerMembraneDef =>
			{
				Membrane innerMembrane = membrane.CreateInnerMembrane();
				membraneList.Add(innerMembrane);
				// Handled now by the NewMembrane event handler.
				// Each membrane needs a system receptor to handle, among other things, the carrier animation.
				// innerMembrane.RegisterReceptor("System", this);
				DeserializeMembranes(innerMembraneDef, innerMembrane, membraneList);
			});
		}

		protected void AddAttribute(XmlNode node, string attrName, string attrValue)
		{
			XmlAttribute attr = node.OwnerDocument.CreateAttribute(attrName);
			attr.Value = attrValue;
			node.Attributes.Append(attr);
		}

		/*
				/// <summary>
				/// TODO: This will eventually become a full fledged dialog.
				/// </summary>
				public void Import(object sender, EventArgs args)
				{
					// Contrast this with the complexities of:
					// http://www.codeproject.com/Articles/685310/Simple-and-fast-CSV-library-in-Csharp
					// http://www.codeproject.com/Articles/415732/Reading-and-Writing-CSV-Files-in-Csharp
					// https://github.com/JoshClose/CsvHelper
					// http://www.codeproject.com/Tips/617843/LINQ-to-CSV
					// And this post is ok but old (not using File.ReadLines): http://stackoverflow.com/questions/1271225/c-sharp-reading-a-file-line-by-line
					// Gotta love the DRY comment in there though -- why would I want to suck in a huge library just to "not repeat" myself.
					// These examples (http://stackoverflow.com/questions/5116604/read-csv-using-linq) don't work because they don't handled quoted strings.

					var data = (from line in File.ReadLines("population.csv")
								let record = line.DelimitedSplit(',')
								select new
								{
									RecordKey = record[0],
									RecordData = record.Sublist(1),
								}).ToList();

					// Parse header for years.
					Dictionary<string, dynamic> types = new Dictionary<string, dynamic>();

					foreach (string year in data[0].RecordData)
					{
						dynamic stsYear = Program.SemanticTypeSystem.Create("Year");
						stsYear.Integer.Value = year.to_i();
						types[year] = stsYear;
					}

					// Parse rows for states.
					foreach (var row in data.Sublist(1))
					{
						dynamic stsState = Program.SemanticTypeSystem.Create("State");
						stsState.Name.Value = row.RecordKey;
						types[row.RecordKey] = stsState;
					}

					// Parse populations.
					foreach (var row in data.Sublist(1))
					{
						row.RecordData.ForEachWithIndex((pop, idx) =>
						{
							dynamic stsPopulation = Program.SemanticTypeSystem.Create("Population");
							stsPopulation.Integer.Value = pop.Replace(",", "").Replace("\"", "").to_i();

							// Create collection ST:
							dynamic stsCollection = Program.SemanticTypeSystem.Create("PopulationByStateByYear");
							stsCollection.Collection.Items.Add(stsPopulation);
							stsCollection.Collection.Items.Add(types[row.RecordKey]);					// The state.
							stsCollection.Collection.Items.Add(types[data[0].RecordData[idx]]);			// The year.
						});
					}

					SymbolTableController.View.UpdateSymbolTable(Program.SemanticTypeSystem.SymbolTable);
					Program.SemanticTypeSystem.FireCreationDone();
				}
		 */

		protected void LogException(Exception ex)
		{
			MessageBox.Show(ex.Message + "\r\n" + Assert.ErrorMessage + "\r\n" + ex.StackTrace, "An error has occurred.", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		// ======================= RECEPTOR LOGIC =============================

		// We are ourselves a receptor!

#pragma warning disable 67
		public event EventHandler<EventArgs> ReceiveProtocolsChanged;
		public event EventHandler<EventArgs> EmitProtocolsChanged;
#pragma warning restore 67

		public string Name { get { return "System"; } }
		public bool IsEdgeReceptor { get { return false; } }
		public bool IsHidden { get { return true; } }

		public IReceptorSystem ReceptorSystem
		{
			get { throw new ApplicationException("A call to get the system receptor container should never be made."); }
			set { throw new ApplicationException("A call to set the system receptor container should never be made."); }
		}

		public string[] GetReceiveProtocols()
		{
			return new string[] { "SystemMessage", "CarrierAnimation", "SystemShowImage", "HaveImageMetadata" };
		}

		public string[] GetEmittedProtocols()
		{
			return new string[] { };
		}

		public void Initialize()
		{
		}

		public void Terminate()
		{
		}

		public void ProcessCarrier(ICarrier carrier)
		{
			switch (carrier.Protocol.DeclTypeName)
			{
				case "SystemMessage":
					{
						string action = carrier.Signal.Action;
						string data = carrier.Signal.Data;
						IReceptorInstance receptorInstance = carrier.Signal.Source;

						if (action == "Flyout")
						{
							VisualizerController.View.Flyout(data, receptorInstance);
						}

						break;
					}

				case "CarrierAnimation":
					{
						Action action = carrier.Signal.Process;
						IReceptorInstance from = carrier.Signal.From;
						IReceptorInstance to = carrier.Signal.To;
						ICarrier outCarrier = carrier.Signal.Carrier;
						VisualizerController.View.AnimateCarrier(action, from, to, outCarrier);
						break;
					}

				case "SystemShowImage":
					{
						IReceptorInstance target = carrier.Signal.From;
						Image image = carrier.Signal.Image;
						VisualizerController.View.AddImage(target, image);
						break;
					}

				case "HaveImageMetadata":
					{
						VisualizerController.View.ProcessImageMetadata(carrier.Signal);
						break;
					}
			}
		}
	}
}
