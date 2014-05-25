using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Clifton.ExtensionMethods;
using Clifton.SemanticTypeSystem;
using Clifton.SemanticTypeSystem.Interfaces;

using TypeSystemExplorer.Controls;
using TypeSystemExplorer.Controllers;
using TypeSystemExplorer.Models;

using Clifton.Tools.Data;

namespace TypeSystemExplorer.Views
{
	public struct CollectionSubtype
	{
		public ISemanticType collection;
		public ISemanticType subinstance;

		public CollectionSubtype(ISemanticType coll, ISemanticType subinst)
		{
			collection = coll;
			subinstance = subinst;
		}
	}

	public class VisualSemanticType
	{
		public string Name { get; protected set; }
		public ISemanticType SemanticType {get; protected set;}
		public Point Location { get; set; }
		public Point StartLocation { get; set; }
		public Point TargetLocation { get; set; }
		public int StepNumber { get; set; }

		public VisualSemanticType(ISemanticType t, string stName)
		{
			SemanticType = t;
			Name = stName;
			Location = new Point(0, 0);
			TargetLocation = new Point(0, 0);
		}
	}

	public class TypeGroup
	{
		public Point Location { get; set; }

		public TypeGroup()
		{
			Location = new Point(0, 0);
		}
	}

	public class VisualizerView : UserControl
	{
		const int RenderTime = 120;

		public ApplicationModel Model { get; protected set; }
		public ApplicationFormController ApplicationController { get; protected set; }
		public VisualizerController Controller { get; protected set; }
		public VisualizerControl Visualizer { get; set; }
		public List<VisualSemanticType> vtypes;
		
		public Dictionary<string, TypeGroup> typeGroups;
		// public Dictionary<ISemanticType, VisualSemanticType> typeVisualizationMap;
		public Dictionary<CollectionSubtype, VisualSemanticType> typeVisualizationMap;
		public List<string> distinctTypes;

		protected Brush blackBrush;
		protected Brush whiteBrush;
		protected Pen pen;
		protected Pen whitePen;
		protected Point origin = new Point(0, 0);
		protected Font font;

		protected Random rnd;
		protected Size objectSize = new Size(20, 20);
		protected List<Pen> penColors;
		protected Timer timer;
		protected List<string> ignoreNames = new List<string>() { "Integer", "Name", "Collection", "Abbreviation", "Year", "Population", "State" };
		protected bool targetState;

		public VisualizerView()
		{
			blackBrush = new SolidBrush(Color.Black);
			whiteBrush = new SolidBrush(Color.White);
			font = new Font(FontFamily.GenericSansSerif, 12);
			pen = new Pen(Color.LightBlue);
			whitePen = new Pen(Color.White);
			penColors = new List<Pen>();

			penColors.Add(new Pen(Color.Red));
			penColors.Add(new Pen(Color.Green));
			penColors.Add(new Pen(Color.Blue));
			penColors.Add(new Pen(Color.Yellow));
			penColors.Add(new Pen(Color.Cyan));
			penColors.Add(new Pen(Color.Magenta));
			penColors.Add(new Pen(Color.Lavender));
			penColors.Add(new Pen(Color.Purple));
			penColors.Add(new Pen(Color.Salmon));

			vtypes = new List<VisualSemanticType>();
			rnd = new Random();
			typeGroups = new Dictionary<string, TypeGroup>();
			typeVisualizationMap = new Dictionary<CollectionSubtype, VisualSemanticType>();
			distinctTypes = new List<string>();
			Program.SemanticTypeSystem.NewSemanticType += OnNewSemanticType;
			Program.SemanticTypeSystem.CreationDone += OnCreationDone;
			
			timer = new Timer();
			timer.Interval = 1000 / 30;		// 30 hz refresh rate in milliseconds;
			timer.Tick += OnTimerTick;
			timer.Start();
		}

		protected void OnTimerTick(object sender, EventArgs e)
		{
			bool more = Step();
			Visualizer.Refresh();

			if (!more)
			{
				targetState = true;
				ResetAllTargets();
			}
		}

		protected bool Step()
		{
			bool more = true;

			//  TODO: These 4 lines are duplicate code.  Refactor into a SizeChanged event handler.
			int hw = Visualizer.ClientRectangle.Width / 2;
			int hh = Visualizer.ClientRectangle.Height / 2;
			int n = (hw < hh ? hw : hh);
			int m = (n / 5);					// the radius of the group's circle.

			vtypes.ForEachWithIndex((vtype, idx) =>
			{
				more = false;	// points to plot
				if (vtype.StepNumber == 0)
				{
					// Initialize the starting location if it's a new entry.

					//if (targetState == false)
					//{
					//	// RANDOM POINT IN SPACE
					//	vtype.TargetLocation = new Point(rnd.Next(-hw, hw), rnd.Next(-hh, hh));
					//}
					//else
					{
						if (vtype.Name == "PopulationByStateByYear")
						{
							// This is a collection.
							List<ISemanticType> stList = (List<ISemanticType>)((dynamic)vtype.SemanticType).Collection.Items;
							int items = stList.Count;

							if (targetState == false)
							{
								// The collection goes somewhere random.
								vtype.TargetLocation = new Point(rnd.Next(-hw, hw), rnd.Next(-hh, hh));
							}

							stList.ForEachWithIndex((st, idx2) =>
							{
								if (targetState == false)
								{
									// Set an initial random point for the item in the collection.
									CollectionSubtype cs = new CollectionSubtype(vtype.SemanticType, st); 
									typeVisualizationMap[cs].TargetLocation = new Point(rnd.Next(-hw, hw), rnd.Next(-hh, hh));
								}
								else
								{
									// Draw each type on the edge of a small circle surrounding the collection type.
									Point p = vtype.TargetLocation;
									Point pOnCircle = GetPointOnCircle(10, idx2, items);
									p.Offset(pOnCircle);
									// p.Offset(hw, hh);										// translate to LL corner at (0, 0)
									// p.Offset(-20, -20);			// UL of the circle offset.
									CollectionSubtype cs = new CollectionSubtype(vtype.SemanticType, st);
									typeVisualizationMap[cs].TargetLocation = p;
								}
							});
						}
					}


					// RANDOM POINT WITHIN A LOCAL GROUP:
					/*
					Point groupPoint = typeGroups[vtype.Name].Location;
					// We can figure out a random target within a circle centered here.
					double randomAngle = rnd.NextDouble() * 3.14159265 * 2;
					Point p = new Point((int)(rnd.Next(-m, m) * Math.Cos(randomAngle)), (int)(rnd.Next(-m, m) * Math.Sin(randomAngle)));
					// Center on the group point.
					p.Offset(groupPoint);
					vtype.TargetLocation = p;
					*/
				
					// RANDOM POINT ON A CIRCLE.
					/*					
					int l = n - n / 6;
					l = (int)(l * (((idx % 3) + 1) / 3.0));		// split each radial into 3 locations along the radial.
					double angle = 3.14159265 * 2 * idx / vtypes.Count;
					Point p = new Point((int)(l * Math.Cos(angle)), (int)(l * Math.Sin(angle)));
					p.Offset(-20, -20);
					vtype.TargetLocation = p;
					*/ 
				}

				if (vtype.StepNumber < RenderTime)
				{
					more = true;
					++vtype.StepNumber;
					// A quasi acceleration curve:
					double stepper = Math.Sin(((3.14159265 / 2.0) * ((double)vtype.StepNumber) / (double)RenderTime));

					// This is the collection.
					vtype.Location = new Point((int)(vtype.StartLocation.X + (vtype.TargetLocation.X - vtype.StartLocation.X) * stepper), ((int)(vtype.StartLocation.Y + (vtype.TargetLocation.Y - vtype.StartLocation.Y) * stepper)));

					// These are the collection items.
					List<ISemanticType> stList = (List<ISemanticType>)((dynamic)vtype.SemanticType).Collection.Items;
					int items = stList.Count;
					stList.ForEach((st) =>
					{
						CollectionSubtype cs = new CollectionSubtype(vtype.SemanticType, st);
						VisualSemanticType vst = typeVisualizationMap[cs];
						vst.Location = new Point((int)(vst.StartLocation.X + (vst.TargetLocation.X - vst.StartLocation.X) * stepper), ((int)(vst.StartLocation.Y + (vst.TargetLocation.Y - vst.StartLocation.Y) * stepper)));
					});
				}
			});

			return more;
		}

		protected void OnNewSemanticType(object sender, NewSemanticTypeEventArgs e)
		{
		}

		protected void OnCreationDone(object sender, EventArgs args)
		{
			ProcessTypeCollection();
		}

		protected void ProcessTypeCollection()
		{
			foreach (SemanticTypeInstance typeInstance in Program.SemanticTypeSystem.Instances.Values)
			{
				ISemanticType instance = typeInstance.Instance;
				string name = typeInstance.Name;

				if (!ignoreNames.Contains(name))
				{
					// A distinct type name, used for coloration logic.
					distinctTypes.AddIfUnique(name);

					// Also, for each type, we have a specific location to group all instances of that type.
					if (!typeGroups.ContainsKey(name))
					{
						typeGroups[name] = new TypeGroup();
						AdjustGroupLocations();
						ResetAllTargets();
					}

					VisualSemanticType vst = new VisualSemanticType(instance, name);
					vtypes.Add(vst);		// Only the collections go in this space.

					// Add all the collection elements.
					if (name == "PopulationByStateByYear")
					{
						// This is a collection.
						List<ISemanticType> stList = (List<ISemanticType>)((dynamic)vst.SemanticType).Collection.Items;
						int items = stList.Count;

						stList.ForEachWithIndex((st, idx2) =>
						{
							// Create the collection types.  This is for every item in the collection, so many will be repeated (like year and state)
							string subname = Program.SemanticTypeSystem.GetSemanticTypeName(st);
							distinctTypes.AddIfUnique(subname);
							VisualSemanticType vst2 = new VisualSemanticType(st, subname);
							CollectionSubtype cs = new CollectionSubtype(instance, st);
							typeVisualizationMap[cs] = vst2;
							// vtypes.Add(vst2);
						});
					}
				}
			}
		}

		/// <summary>
		/// Force all types to recalculate their target locations.
		/// </summary>
		protected void ResetAllTargets()
		{
			vtypes.ForEach(t =>
				{
					t.StepNumber = 0;
					t.StartLocation = t.Location;

					string name = Program.SemanticTypeSystem.GetSemanticTypeName(t.SemanticType);

					if (name == "PopulationByStateByYear")
					{

						List<ISemanticType> stList = (List<ISemanticType>)((dynamic)t.SemanticType).Collection.Items;

						stList.ForEach((st) =>
						{
							// W're going to draw the lines between each item in the collection.
							CollectionSubtype cs = new CollectionSubtype(t.SemanticType, st);
							typeVisualizationMap[cs].StartLocation = typeVisualizationMap[cs].Location;
						});
					}
				});
		}

		protected void AdjustGroupLocations()
		{
			// Groups are located on a circle the center of which is at 0,0, the middle of the visualizer window.
			int hw = Visualizer.ClientRectangle.Width / 2;
			int hh = Visualizer.ClientRectangle.Height / 2;
			int n = (hw < hh ? hw : hh);
			n -= n/5;			// moves in by 1/5 of the minimum of the width or height.
			//n = n / 2;

			int numGroups = typeGroups.Count;

			typeGroups.Values.ForEachWithIndex((tg, idx) =>
			{
				// Each group gets placed equidistant along the edge of a circle.
				tg.Location = GetPointOnCircle(n, idx, numGroups);
			});
		}

		/// <summary>
		/// Returns a point on the circle for a given index, number of points to plot on the circle, and the radius.
		/// </summary>
		protected Point GetPointOnCircle(int radius, int idx, int totalPoints)
		{
			double angle = (2.0 * 3.1415926535 * idx) / totalPoints;
			Point p = new Point((int)(radius * Math.Cos(angle)), (int)(radius * Math.Sin(angle)));

			return p;
		}

		protected void OnVisualizerPaint(object sender, PaintEventArgs e)
		{
			Control ctrl = (Control)sender;
			int hw = ctrl.ClientRectangle.Width / 2;
			int hh = ctrl.ClientRectangle.Height / 2;

			e.Graphics.FillRectangle(blackBrush, new Rectangle(Location, Size));
/*
			// Plot an "X" where the group coordinates are located.
			foreach (TypeGroup tg in typeGroups.Values)
			{
				Point p = tg.Location;
				p.Offset(hw, hh);
				e.Graphics.DrawString("X", font, whiteBrush, p);
			}
*/
			foreach (VisualSemanticType vtype in vtypes)
			{

				if (vtype.Name == "PopulationByStateByYear")
				{
					// This is a collection.
					List<ISemanticType> stList = (List<ISemanticType>)((dynamic)vtype.SemanticType).Collection.Items;
					int items = stList.Count;
					Point[] objects = new Point[items];

					stList.ForEachWithIndex((st, idx) =>
					{
						// W're going to draw the lines between each item in the collection.
						CollectionSubtype cs = new CollectionSubtype(vtype.SemanticType, st);
						Point p = typeVisualizationMap[cs].Location;
						p.Offset(objectSize.Width / 2, objectSize.Height / 2);		// UL corner of the area for the circle.
						p.Offset(hw, hh);										// translate to LL corner at (0, 0)
						p.Offset(-20, -20);			// UL of the circle offset.
						objects[idx] = p;

						// Draw each type on the edge of a small circle surrounding the collection type.
						//string name = Program.SemanticTypeSystem.GetSemanticTypeName(st);
						//distinctTypes.AddIfUnique(name);
						//Pen pen = penColors[distinctTypes.IndexOf(name) % distinctTypes.Count];
						//Point p = vtype.Location;
						//Point pOnCircle = GetPointOnCircle(10, idx, items);
						//p.Offset(pOnCircle);
						//p.Offset(hw, hh);										// translate to LL corner at (0, 0)
						//p.Offset(-20, -20);			// UL of the circle offset.
						//objects[idx] = p;
						//e.Graphics.DrawEllipse(pen, new Rectangle(p, objectSize));
					});

					stList.ForEachWithIndex((st, idx) =>
					{
						string subname = Program.SemanticTypeSystem.GetSemanticTypeName(st);

						int n = (idx + 1) % items;
						Point p1 = objects[idx];
						Point p2 = objects[n];
						p1.Offset(10, 10);
						p2.Offset(10, 10);
						e.Graphics.DrawLine(whitePen, p1, p2);

						Pen pen = penColors[distinctTypes.IndexOf(subname) % distinctTypes.Count];
						Point p = objects[idx];
						e.Graphics.DrawEllipse(pen, new Rectangle(p, objectSize));
					});
				}

				// else
				{
					// This is just the collection instance.
					Pen pen = penColors[distinctTypes.IndexOf(vtype.Name) % distinctTypes.Count];
					Point p = vtype.Location;
					p.Offset(objectSize.Width / 2, objectSize.Height / 2);		// UL corner of the area for the circle.
					p.Offset(hw, hh);										// translate to LL corner at (0, 0)
					p.Offset(-20, -20);			// UL of the circle offset.
					e.Graphics.DrawEllipse(pen, new Rectangle(p, objectSize));
				}
 
			}
		}
	}
}
