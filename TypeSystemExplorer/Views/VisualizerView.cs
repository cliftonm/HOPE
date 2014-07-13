#define VIVEK
// #define REMOVE_EMPTY_MEMBRANES

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using Clifton.Drawing;
using Clifton.ExtensionMethods;
using Clifton.MycroParser;
using Clifton.Tools.Data;
using Clifton.Tools.Strings.Extensions;

using Clifton.Receptor;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem;
using Clifton.SemanticTypeSystem.Interfaces;


using TypeSystemExplorer.Controls;
using TypeSystemExplorer.Controllers;
using TypeSystemExplorer.Models;

namespace TypeSystemExplorer.Views
{
	public class ConfigurationInfo
	{
		public IReceptor Receptor { get; set; }
		public MycroParser Parser { get; set; }
	}

	// From http://www.differentpla.net/content/2005/02/using-propertygrid-with-dictionary
	class DictionaryPropertyGridAdapter : ICustomTypeDescriptor
	{
		IDictionary _dictionary;

		public DictionaryPropertyGridAdapter(IDictionary d)
		{
			_dictionary = d;
		}
		public string GetComponentName()
		{
			return TypeDescriptor.GetComponentName(this, true);
		}

		public EventDescriptor GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(this, true);
		}

		public string GetClassName()
		{
			return TypeDescriptor.GetClassName(this, true);
		}

		public EventDescriptorCollection GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(this, attributes, true);
		}

		EventDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetEvents()
		{
			return TypeDescriptor.GetEvents(this, true);
		}

		public TypeConverter GetConverter()
		{
			return TypeDescriptor.GetConverter(this, true);
		}

		public object GetPropertyOwner(PropertyDescriptor pd)
		{
			return _dictionary;
		}

		public AttributeCollection GetAttributes()
		{
			return TypeDescriptor.GetAttributes(this, true);
		}

		public object GetEditor(Type editorBaseType)
		{
			return TypeDescriptor.GetEditor(this, editorBaseType, true);
		}

		public PropertyDescriptor GetDefaultProperty()
		{
			return null;
		}

		PropertyDescriptorCollection
			System.ComponentModel.ICustomTypeDescriptor.GetProperties()
		{
			return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
		}

		public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			ArrayList properties = new ArrayList();
			foreach (DictionaryEntry e in _dictionary)
			{
				properties.Add(new DictionaryPropertyDescriptor(_dictionary, e.Key));
			}

			PropertyDescriptor[] props =
				(PropertyDescriptor[])properties.ToArray(typeof(PropertyDescriptor));

			return new PropertyDescriptorCollection(props);
		}
	}

	class DictionaryPropertyDescriptor : PropertyDescriptor
	{
		IDictionary _dictionary;
		object _key;

		internal DictionaryPropertyDescriptor(IDictionary d, object key)
			: base(key.ToString(), null)
		{
			_dictionary = d;
			_key = key;
		}

		public override Type PropertyType
		{
			get 
			{ 
				Type ret = typeof(string);

				object val = _dictionary[_key];
				if (val != null)
				{
					ret = _dictionary[_key].GetType();
				}

				return ret;
			}
		}

		public override void SetValue(object component, object value)
		{
			_dictionary[_key] = value;
		}

		public override object GetValue(object component)
		{
			return _dictionary[_key];
		}

		public override bool IsReadOnly
		{
			get { return true; }
		}

		public override Type ComponentType
		{
			get { return null; }
		}

		public override bool CanResetValue(object component)
		{
			return false;
		}

		public override void ResetValue(object component)
		{
		}

		public override bool ShouldSerializeValue(object component)
		{
			return false;
		}
	}

	public struct Line
	{
		public Point P1 {get;set;}
		public Point P2 {get;set;}

		public static bool operator ==(Line a, Line b)
		{
			return (a.P1 == b.P1) && (a.P2 == b.P2);
		}

		public static bool operator !=(Line a, Line b)
		{
			return (a.P1 != b.P1) || (a.P2 != b.P2);
		}

		public override bool Equals(object obj)
		{
			return ((Line)obj) == this;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	public class FlyoutItem
	{
		public string Text { get; set; }
		public Point Location { get; set; }
		public Size Direction { get; set; }
	}

	public class CarrierAnimationItem
	{
		public Action OnArrivalDo { get; set; }
		public Point StartPosition { get; set; }
		public Rectangle CurrentRegion { get; set; }
		public IReceptorInstance Target { get; set; }
		public ICarrier Carrier { get; set; }
		public int CurveIndex { get; set; }

		public CarrierAnimationItem()
		{
		}
	}

	public class ImageMetadata
	{
		public Image Image { get; set; }
		public List<MetadataPacket> MetadataPackets { get; protected set; }

		public ImageMetadata()
		{
			MetadataPackets = new List<MetadataPacket>();
		}
	}

	public class MetadataPacket
	{
		// metadata with a protocol is actionable.
		public string ProtocolName { get; set; }
		public string Name { get; set; }
		public string Value { get; set; }
		public string PropertyName { get; set; }
	}

	public class CarouselState
	{
		public int Offset { get; set; }
		public List<ImageMetadata> Images { get; set; }
		public string ActiveImageFilename { get; set; }
		public Rectangle ActiveImageLocation { get; set; }
		public int ActiveImageIndex { get; set; }

		public CarouselState()
		{
			Images = new List<ImageMetadata>();
		}
	}

	public class Circle
	{
		public Point Center { get; set; }
		public int Radius { get; set; }
	}

	public struct Connection
	{
		public string Protocol { get; set; }
		public Line Line { get; set; }
	}

	public class VisualizerView : UserControl
	{
		const int RenderTime = 120;
		const int CarrierTime = 5; // 5 for 1/4 second delay, 25 for 1 second delay.  50 for 2 second delay.
		const int OrbitCountMax = 50;
		const int MetadataHeight = 15;	// the row height for metadata text.
		const int MembraneNubRadius = 10;

		protected Size ReceptorSize = new Size(40, 40);
		protected Size ReceptorHalfSize = new Size(20, 20);
		protected Point dropPoint;
		protected bool showMembranes = true;

		public ApplicationModel Model { get; protected set; }
		public ApplicationFormController ApplicationController { get; protected set; }
		public VisualizerController Controller { get; protected set; }
		public VisualizerControl Visualizer { get; set; }

		public bool StartDrop { get; set; }
		public bool ShowMembranes
		{
			get { return ShowMembranes; }
			set
			{
				showMembranes = value;
				
				if (showMembranes)
				{
					RecalcMembranes();
					Invalidate(true);
				}
			}
		}

		/// <summary>
		/// Sets the drop point, converting from screen coordinates to client coordinates.
		/// </summary>
		public Point DropPoint
		{
			get { return dropPoint; }
			set { dropPoint = PointToClient(value); }
		}

		/// <summary>
		/// Sets the dropp point given a client coordinate.
		/// </summary>
		public Point ClientDropPoint
		{
			set { dropPoint = value; }
		}

		protected Dictionary<IReceptor, Point> receptorLocation;
		protected Dictionary<IMembrane, Circle> membraneLocation;

		protected List<FlyoutItem> flyouts;
		protected List<CarrierAnimationItem> carrierAnimations;
		protected Dictionary<IReceptor, CarouselState> carousels;
		protected List<Connection> receptorConnections;

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
		
		// Mouse capture / recepture moving fields:
		protected bool movingReceptor;
		protected bool movingMembrane;
		protected bool rubberBand;
		protected bool dragSurface;
		protected IReceptor selectedReceptor;
		protected IMembrane selectedMembrane;
		protected Point surfaceOffset = new Point(0, 0);
		protected Point mouseStart;
		protected Point mousePosition;
		protected DateTime mouseHoverStartTime;

		protected IMembrane membraneBeingConfigured;

		protected Image pauseButton;
		protected Image playButton;

		protected int orbitCount = 0;
		protected bool paused;

		// When the membrane is being moved, keeps a short of list of mouse offsets
		// to determine whether the membrane is being "shaken" left-right.
		protected DateTime shakeStart;
		protected int shakeCurrentDirection;
		protected int shakeCount;
		protected bool shakeOK;			// Used to stop further "pops".

		protected Pen receptorLineColor = new Pen(Color.Cyan); // new Pen(Color.FromArgb(40, 40, 60));
		protected Pen receptorLineColor2 = new Pen(Color.Orange); // new Pen(Color.FromArgb(40, 40, 60));
		protected Pen receptorLineColor3 = new Pen(Color.Pink); // new Pen(Color.FromArgb(40, 40, 60));

		public VisualizerView()
		{
			blackBrush = new SolidBrush(Color.Black);
			whiteBrush = new SolidBrush(Color.White);
			font = new Font(FontFamily.GenericSansSerif, 8);
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

			playButton = Image.FromFile("play.bmp");
			pauseButton = Image.FromFile("pause.bmp");

			InitializeCollections();

			rnd = new Random();

			Program.Skin.NewReceptor += OnNewReceptor;
			Program.Skin.NewCarrier += OnNewCarrier;
			Program.Skin.ReceptorRemoved += OnReceptorRemoved;
			Program.Skin.NewMembrane += OnNewMembrane;

			timer = new Timer();
			timer.Interval = 1000 / 30;		// 30 hz refresh rate in milliseconds;
			timer.Tick += OnTimerTick;
			timer.Start();
		}

		public Point GetLocation(IReceptor r)
		{
			return receptorLocation[r];
		}

		public Circle GetLocation(IMembrane m)
		{
			return membraneLocation[m];
		}

		public void Reset()
		{
			InitializeCollections();
			Invalidate(true);
		}

		public void UpdateConnections()
		{
			CreateReceptorConnections();
			RecalcMembranes();
			membraneLocation.Keys.ForEach(m => m.ProcessQueuedCarriers());
			Program.Skin.ProcessQueuedCarriers();
			Invalidate(true);
		}

		public void Stop()
		{
			timer.Stop();
			timer.Dispose();
		}

		protected void InitializeCollections()
		{
			receptorLocation = new Dictionary<IReceptor, Point>();
			membraneLocation = new Dictionary<IMembrane, Circle>();
			flyouts = new List<FlyoutItem>();
			carrierAnimations = new List<CarrierAnimationItem>();
			carousels = new Dictionary<IReceptor, CarouselState>();
			receptorConnections = new List<Connection>();
		}

		public void Flyout(string msg, IReceptorInstance receptorInstance)
		{
			Point p = receptorLocation.Single(kvp => kvp.Key.Instance == receptorInstance).Value;
			flyouts.Add(new FlyoutItem() { Text = msg, Location = p, Direction = new Size(5, -3) });
		}

		public void AnimateCarrier(Action action, IReceptorInstance from, IReceptorInstance to, ICarrier carrier)
		{
			// Assign an unassigned carrier to the target.
			CarrierAnimationItem c = carrierAnimations.SingleOrDefault(q => (q.Carrier == carrier) && (q.Target == null) );

			if (c == null)
			{
				// Oops.  The carrier animation has been consumed, but there are more receptors for this carrier, so we need to add another carrier animation now.
				// Get an existing animation for this carrier.  We don't care what carrier we get, because they're all going to be originating from the same receptor.
				// TODO: This is not necessarily true.  The same carrier could be sourced by different receptors!
				CarrierAnimationItem existing = carrierAnimations.FirstOrDefault(q => (q.Carrier == carrier) && (q.Target != null));

				if (existing != null)
				{
					// Use an existing carrier animation from the source receptor to get the starting position.
					carrierAnimations.Add(new CarrierAnimationItem() { StartPosition = existing.StartPosition, OnArrivalDo = action, Target = to, Carrier = carrier });
				}
				else
				{
					// else -- hmmm.. Why don't we have an existing carrier animation instance already???
					// We have a carrier that wasn't registered by our event handler, so we have absolutely no clue where it goes.  Place it randomly.
					// These are the CarrierAnimation protocols that we really don't care about.
					// Point p = new Point(rnd.Next(Width), rnd.Next(Height));
					// carrierAnimations.Add(new CarrierAnimationItem() { StartPosition = p, OnArrivalDo = action, Target = to, Carrier = carrier });
				}
			}
			else
			{
				// Use the carrier animation we already have an instance of.
				c.OnArrivalDo = action;
				c.Target = to;
			}
			/*
			if (from == null)
			{
				// From the system edge receptor, so drop the carriers randomly onto the surface.
				Point p = new Point(rnd.Next(Width), rnd.Next(Height));
				carrierAnimations.Add(new CarrierAnimationItem() { StartPosition = p, OnArrivalDo = action, Target = to, Carrier=carrier });
			}
			else
			{
				Point p = receptorLocation.Single(kvp => kvp.Key.Instance == from).Value;
				carrierAnimations.Add(new CarrierAnimationItem() { StartPosition = p, OnArrivalDo = action, Target = to, Carrier = carrier });
			}
			 */
		}

		public void AddImage(IReceptorInstance receptorInstance, Image image)
		{
			IReceptor r = receptorLocation.Single(kvp => kvp.Key.Instance == receptorInstance).Key;
			CarouselState cstate;

			if (!carousels.TryGetValue(r, out cstate))
			{
				cstate = new CarouselState();
				carousels[r] = cstate;
			}

			if (cstate.Images.Count < 100)
			{
				ImageMetadata imeta = new ImageMetadata() { Image = image };
				cstate.Images.Add(imeta);

				GetImageMetadata(r, imeta);
				Invalidate(true);
			}
			else
			{
				// Disable the receptor -- too many images.
				// TODO: Better way of indicating this to the user.
				// TODO: We want to limit the total number of images across all viewers.
				r.Enabled = false;
				Invalidate(true);
			}
		}

		public Point GetRandomLocation()
		{
			return new Point(rnd.Next(ClientRectangle.Width - 80) + 40, rnd.Next(ClientRectangle.Height - 80) + 40);
		}

		public void ProcessImageMetadata(dynamic signal)
		{
			ICarrier metadata = signal.Metadata;
			string protocol = metadata.Protocol.DeclTypeName;
			string path = signal.ImageFilename.Filename;
			string fn = Path.GetFileName(path);
			// Get the the first carousel key-value pair (Receptor, CarouselState) which contains the image for which we received the metadata.
			var carousel = carousels.FirstOrDefault(kvp => kvp.Value.Images.Count(imeta => Path.GetFileName(imeta.Image.Tag.ToString().Surrounding("-thumbnail")) == fn) > 0);

			// The user could have removed the viewer by the time we get a response.
			if (carousel.Value != null)
			{
				// Get the metadata packet for this image.
				List<MetadataPacket> packets = carousel.Value.Images.First(m => Path.GetFileName(m.Image.Tag.ToString().Surrounding("-thumbnail")) == fn).MetadataPackets;
				InitializeMetadata(protocol, packets, metadata.Signal);
				Invalidate(true);
			}
		}

		/// <summary>
		/// Return the innermost selected membrane by comparing the test point to the membrane nub's radius.
		/// Return null if no membrane found. 
		/// By default, the test point must be within the nub radius.  If useNubRadius is false, then the membrane radius is used
		/// to qualify the test point.
		/// </summary>
		public Membrane FindInnermostSelectedMembrane(Point testPoint, Membrane m, bool useNubRadius = true)
		{
			Membrane ret = null;

			// TODO: We wouldn't need this test if the skin were always part of the membrane location map.
			if (membraneLocation.ContainsKey(m))
			{
				// Use either the nub radius or the membrane radius, depending on the flag.  
				int radius = (useNubRadius ? MembraneNubRadius : membraneLocation[m].Radius);

				if (CircleToBoundingRectangle(membraneLocation[m].Center, radius).Contains(testPoint))
				{
					ret = m;
				}
			}

			foreach (Membrane child in m.Membranes)
			{
				Membrane recurseRet = FindInnermostSelectedMembrane(testPoint, (Membrane)child, useNubRadius);
				recurseRet.IfNotNull(r => ret = r);
			}

			return ret;
		}

		// This is complex piece of code.
		/// <summary>
		/// Gets the metadata tags reflectively, so that we have a general purpose function for display image metadata.
		/// </summary>
		protected void InitializeMetadata(string protocol, List<MetadataPacket> packets, dynamic signal)
		{
			packets.Clear();
			// Get all the native and semantic types so we can get the values of these types from the signal.
			List<IGetSetSemanticType> types = Program.SemanticTypeSystem.GetSemanticTypeStruct(protocol).AllTypes;
			// Get the type of the signal for reflection.
			Type t = signal.GetType();
			// For each property in the signal where the value of the property isn't null (this check may not be necessary)...
			t.GetProperties().Where(p => p.GetValue(signal) != null).ForEach(p =>
				{
					// We get the value, which is either a NativeType or SemanticElement
					object obj = p.GetValue(signal);
					string itemProtocol = null;

					// If it's a SemanticElement, then we have a protocol that we can use for actionable metadata.
					// We would package up this protocol into a carrier with the metadata signal in order to let
					// other receptors process the protocol.
					if (obj is IRuntimeSemanticType)
					{
						itemProtocol = p.Name;
					}

					// Here we the IGetSetSemanticType instance (giving us access to Name, GetValue and SetValue operations) for the type.  
					IGetSetSemanticType protocolType = types.Single(ptype => ptype.Name == itemProtocol);
					// Create a metadata packet.
					MetadataPacket metadataPacket = new MetadataPacket() { ProtocolName = itemProtocol, Name = p.Name};

					if (protocolType is ISemanticElement)
					{
						metadataPacket.PropertyName = ((ISemanticElement)protocolType).GetImplementingName(Program.SemanticTypeSystem);
					}

					// Get the object value.  This does some fancy some in the semantic type system,
					// depending on whether we're dealing with a native type (simple) or a semantic element (complicated).
					object val = protocolType.GetValue(Program.SemanticTypeSystem, signal);
					// If the type value isn't null, then we have some metadata we can display for the image.
					val.IfNotNull(v =>
						{
							metadataPacket.Value = v.ToString();

							if (!String.IsNullOrEmpty(metadataPacket.Value))
							{
								packets.Add(metadataPacket);
							}
						});
				});
		}

		protected void GetImageMetadata(IReceptor r, ImageMetadata imeta)
		{
			Image img = imeta.Image;
			ISemanticTypeStruct protocol = Program.SemanticTypeSystem.GetSemanticTypeStruct("GetImageMetadata");
			dynamic signal = Program.SemanticTypeSystem.Create("GetImageMetadata");
			// Remove any "-thumbnail" so we get the master image.
			signal.ImageFilename.Filename = Path.GetFileName(img.Tag.ToString().Surrounding("-thumbnail"));
			// signal.ResponseProtocol = "HaveImageMetadata";
			GetReceptorMembrane(r).CreateCarrierIfReceiver(r.Instance, protocol, signal);
		}

		protected void OnTimerTick(object sender, EventArgs e)
		{
			if (!paused)
			{
				bool more = Step();

				if (more)
				{
					Visualizer.Refresh();
				}
			}

			CheckMouseHover();
		}

		protected bool Step()
		{
			bool more = ProcessFlyouts();
			more = ProcessCarrierAnimations() | more;			// Always "or" at the end, so that ProcessCarrierAnimations gets called.

			return more;
		}
		
		protected bool ProcessFlyouts()
		{
			bool more = flyouts.Count > 0;

			List<FlyoutItem> remove = new List<FlyoutItem>();

			foreach (FlyoutItem item in flyouts)
			{
				item.Location = Point.Add(item.Location, item.Direction);

				// More to do if any flyout is still onscreen.
				if (!(new Rectangle(new Point(0, 0), Size).Contains(item.Location)))
				{
					remove.Add(item);
				}
			}

			if (remove.Count > 0)
			{
				remove.ForEach(r => flyouts.Remove(r));
			}

			return more;
		}

		protected bool ProcessCarrierAnimations()
		{
			bool more = carrierAnimations.Any(c => c.Target != null);

			List<CarrierAnimationItem> remove = new List<CarrierAnimationItem>();

			// collection can be added by actions, so use an indexer instead
			for (int i=0; i<carrierAnimations.Count; i++)
			{
				CarrierAnimationItem item = carrierAnimations[i];

				if (item.Target != null)
				{
					++item.CurveIndex;

					if (item.CurveIndex == CarrierTime)
					{
						remove.Add(item);
						item.OnArrivalDo();
					}
				}
			}

			if (remove.Count > 0)
			{
				remove.ForEach(r => carrierAnimations.Remove(r));
			}

			return more;
		}

		/// <summary>
		/// Wire up events that we want to listen to for this new membrane.
		/// </summary>
		protected void OnNewMembrane(object sender, MembraneEventArgs e)
		{
			Membrane m = (Membrane)e.Membrane;
			m.NewMembrane += OnNewMembrane;
			m.NewReceptor += OnNewReceptor;
			m.NewCarrier += OnNewCarrier;
			m.ReceptorRemoved += OnReceptorRemoved;

			// Placeholder only.
			// Because membranes are dynamic in size based on their receptor locations, 
			// the current membrane location is always recalculated when a receptor moves.
			membraneLocation[m] = new Circle() { Center = new Point(-1, -1), Radius = 0 };
		}

		protected void OnNewReceptor(object sender, ReceptorEventArgs e)
		{
			// int hw = ctrl.ClientRectangle.Width / 2;
			// int hh = ctrl.ClientRectangle.Height / 2;
			Point p = dropPoint;

			if (!StartDrop)
			{
				p = GetRandomLocation();
			}

			if (!e.Receptor.Instance.IsHidden)
			{
				e.Receptor.Instance.EmitProtocolsChanged += ProtocolsChanged;
				e.Receptor.Instance.ReceiveProtocolsChanged+= ProtocolsChanged;

				receptorLocation[e.Receptor] = p;
				CreateReceptorConnections();
				RecalcMembranes();
				Invalidate(true);
			}
		}

		protected void ProtocolsChanged(object sender, EventArgs e)
		{
			CreateReceptorConnections();
			Program.Skin.ProcessQueuedCarriers();
			Invalidate(true);
		}

		protected void OnNewCarrier(object sender, NewCarrierEventArgs e)
		{
			// Make sure this isn't a system message, which is a hidden receptor.
			// TODO: We need to check if any receptors exist and whether any are hidden or not.  If it's hidden, then we don't create a carrier animation instance.
			if (!ApplicationController.GetReceiveProtocols().Select(rp=>rp.Protocol).Contains(e.Carrier.Protocol.DeclTypeName))
			{
				if (e.From == Program.Skin["System"].Instance)
				{
					Point p = dropPoint;
					
					if (!StartDrop)
					{
						// From the system edge receptor, so drop the carriers randomly onto the surface.
						p = GetRandomLocation();
					}

					carrierAnimations.Add(new CarrierAnimationItem() { StartPosition = p, Carrier = e.Carrier });
				}
				else
				{
					Point p = receptorLocation.Single(kvp => kvp.Key.Instance == e.From).Value;
					p.Offset((int)(ReceptorSize.Width * Math.Cos(Math.PI * 2.0 * orbitCount / OrbitCountMax)) - 3, (int)(ReceptorSize.Height * Math.Sin(Math.PI * 2.0 * orbitCount / OrbitCountMax)) - 3);
					carrierAnimations.Add(new CarrierAnimationItem() { StartPosition = p, Carrier = e.Carrier });
					orbitCount = (orbitCount + 1) % OrbitCountMax;
				}

				Invalidate(true);
			}
		}

		protected void OnReceptorRemoved(object sender, ReceptorEventArgs e)
		{
			// Also remove the receptor for our local collections.
			receptorLocation.Remove(e.Receptor);
			carousels.Remove(e.Receptor);
			carrierAnimations.RemoveAll(a => a.Target == e.Receptor.Instance);
			CreateReceptorConnections();
			RecalcMembranes();
			Invalidate(true);
		}

		protected void MouseDownEvent(object sender, MouseEventArgs args)
		{
			Point testPoint = NegativeSurfaceOffsetAdjust(args.Location);

			if (args.Button == MouseButtons.Left)
			{
				CheckPlayPauseButtons(args.Location);

				var selectedReceptors = receptorLocation.Where(kvp => CircleToBoundingRectangle(kvp.Value, ReceptorSize.Width/2).Contains(testPoint));

				if (selectedReceptors.Count() > 0)
				{
					selectedReceptor = selectedReceptors.First().Key;
					movingReceptor = true;
					mouseStart = args.Location;

					// Setup for vertical shake test.
					shakeStart = DateTime.Now;
					shakeCurrentDirection = 0;
					shakeCount = 0;
					shakeOK = true;
				}
				else
				{
					// We also want to check if the mouse is also on a membrane nub.  This affects certain operations.
					// Sometimes membranes can contain other membranes where the outer membrane has no receptors, resulting in the nubs being in exactly the same locations.
					// We always want to initiate with the innermost membrane in that case.
					selectedMembrane = FindInnermostSelectedMembrane(testPoint, Program.Skin);

					if (selectedMembrane != null)
					{
						// Setup for horizontal shake test.
						shakeStart = DateTime.Now;
						shakeCurrentDirection = 0;
						shakeCount = 0;
						shakeOK = true;
						movingMembrane = true;
						mouseStart = args.Location;
					}
					else
					{
						// If neither membrane nor receptor is selected, then go into rubberband mode.
						selectedReceptor = null;
						rubberBand = true;
						mouseStart = args.Location;
						mousePosition = args.Location;
					}
				}
			}
			else if (args.Button == MouseButtons.Right)
			{
				// If no membrane is selected, move the entire surface.
				var selectedMembranes = membraneLocation.Where(kvp => CircleToBoundingRectangle(SurfaceOffsetAdjust(kvp.Value.Center), MembraneNubRadius).Contains(testPoint));

				// Only move the surface if no membrane is selected.  
				// TODO: Do we really need to do this?
				if (selectedMembranes.Count() == 0)
				{
					dragSurface = true;
					mouseStart = NegativeSurfaceOffsetAdjust(args.Location);
					mousePosition = NegativeSurfaceOffsetAdjust(args.Location);
				}
			}
		}

		protected static Rectangle playButtonRect = new Rectangle(0, 0, 32, 32);
		protected static Rectangle pauseButtonRect = new Rectangle(35, 0, 32, 32);

		protected void CheckPlayPauseButtons(Point p)
		{
			if (playButtonRect.Contains(p))
			{
				paused = false;
			}
			else if (pauseButtonRect.Contains(p))
			{
				paused = true;
			}
		}

		protected void MouseUpEvent(object sender, MouseEventArgs args)
		{
			dragSurface = false;
			// Do not adjust this point for surface offsets, as this is the mouse location and
			// we're comparing it to the client rectangle.
			Point testPoint = args.Location;

			// Moving a receptor always takes priority.
			if (movingReceptor)
			{
				movingReceptor = false;

				if (!ClientRectangle.Contains(testPoint))
				{
					// Remove the receptor completely from the surface.
					GetReceptorMembrane(selectedReceptor).Remove(selectedReceptor);
					// Cleaning up our collections will happen when the ReceptorRemoved event fires.
				}
				else
				{
					// If the final position for the receptor is in a different membrane, move the receptor there.
					Membrane sourceMembrane = GetReceptorMembrane(selectedReceptor);
					Membrane destMembrane = FindInnermostSelectedMembrane(testPoint, Program.Skin, false);
					destMembrane.IfNull(() => destMembrane = Program.Skin);
					// Did the receptor move within the same membrane?  If so, ignore it, otherwise
					// move the receptor to the innermost membrane.
					if (sourceMembrane != destMembrane)
					{
						sourceMembrane.MoveReceptorToMembrane(selectedReceptor, destMembrane);
						CreateReceptorConnections(); 
						RecalcMembranes();
						Invalidate(true);
					}
				}

				selectedReceptor = null;
			}
			
			if (movingMembrane)
			{
				movingMembrane = false;
				selectedMembrane = null;
			}

			if (rubberBand)
			{
				// Gather and contained receptors into a membrane.
				// mouseStart and mousePosition define the rectangle.
				rubberBand = false;

				// The containing rectangle.
				Rectangle r = Rectangle.FromLTRB(Math.Min(mouseStart.X, mousePosition.X), Math.Min(mouseStart.Y, mousePosition.Y), Math.Max(mouseStart.X, mousePosition.X), Math.Max(mouseStart.Y, mousePosition.Y));
				Rectangle testR = new Rectangle(NegativeSurfaceOffsetAdjust(r.Location), r.Size);

				// Get receptors inside the rectangle, ignoring any hidden receptors--a defensive measure if we ever decide to show the system receptor.
				List<IReceptor> receptors = receptorLocation.Where(kvp => testR.Contains(kvp.Value) && !kvp.Key.Instance.IsHidden).Select(kvp => kvp.Key).ToList();

				// Do we have any?
				if (receptors.Count() > 0)
				{
					// Verify that they are all currently contained within a single membrane.
					// The Skin membrane will return all the membranes containing this list of receptors.
					List<Membrane> membranes = Program.Skin.GetMembranesContaining(receptors);

					if (membranes.Count == 1)
					{
						Membrane membrane = membranes[0];
						Membrane innerMembrane = membrane.CreateInnerMembrane();
						membrane.MoveReceptorsToMembrane(receptors, innerMembrane);
						CreateReceptorConnections();
						RecalcMembranes();
					}
					else
					{
						// We are including receptors contained within other membranes.  
						// The idea here is to create a membrane containing those membranes as well as any non-contained receptors.
						// For this, we need to know the outer membrane:

					}
				}
				// else no receptors selected.

				Invalidate(true);
			}
		}

		protected void MouseMoveEvent(object sender, MouseEventArgs args)
		{
			mousePosition = args.Location;

			if (movingReceptor)
			{
				base.OnMouseMove(args);
				Point offset = Point.Subtract(args.Location, new Size(mouseStart));

				// If vertically shook, the receptor will move to the parent membrane.
				if (shakeOK && VerticalShakeTest(offset))
				{
					shakeOK = false;			// User must release and start again.
					// Receptors always belong to membranes
					Membrane m = GetReceptorMembrane(selectedReceptor);

					// And the membrane has a parent (not skin)...
					if (m.ParentMembrane != null)
					{
						// Move the receptor to the parent membrane.
						m.MoveReceptorToMembrane(selectedReceptor, m.ParentMembrane);
						Point curPos = receptorLocation[selectedReceptor];
						receptorLocation[selectedReceptor] = Point.Add(curPos, new Size(offset));
						mouseStart = args.Location;

						// If the membrane and all its children now have no receptors, we will remove it.
						if (!MembraneOrChildrenHaveReceptors(m))
						{
							m.Dissolve();
							RemoveMembranes(m);
						}

						CreateReceptorConnections();
						RecalcMembranes();
						Invalidate(true);
					}
				}
				else
				{
					Point curPos = receptorLocation[selectedReceptor];
					receptorLocation[selectedReceptor] = Point.Add(curPos, new Size(offset));
					mouseStart = args.Location;
					CreateReceptorConnections();
					RecalcMembranes();
					Invalidate(true);
				}
			}
			else if (movingMembrane)
			{
				// IMPORTANT! You cannot dissolve a membrane that contains a single receptor.  You must remove the receptor first.
				// TODO: Fix this at some point, so that both vert. and horiz. shaking can be tested without affecting each other.
				base.OnMouseMove(args);
				Point offset = Point.Subtract(args.Location, new Size(mouseStart));

				// If horizontally shook, the mebrane will disolve.
				if (shakeOK && HorizontalShakeTest(offset))
				{
					shakeOK = false;			// User must release and start again.
					selectedMembrane.Dissolve();
					RemoveMembranes(selectedMembrane);
					CreateReceptorConnections();
					RecalcMembranes();
					Invalidate(true);
				}
				else
				{
					// To move the membrane, we actually move each receptor inside the membrane.
					// The exception is when there are no receptors in the membrane or its children, 
					// Move receptors in this membrane and all inner membranes.
					MoveReceptors(selectedMembrane, offset);
					// Also move the membrane and child membranes, as this handles empty children.
					MoveMembranes(selectedMembrane, offset);

					mouseStart = args.Location;
					CreateReceptorConnections();
					RecalcMembranes();
					Invalidate(true);
				}
			}
			else if (rubberBand)
			{
				// Redraw the rubberband rectangle.
				Invalidate(true);
			}
			else if (dragSurface)
			{
				base.OnMouseMove(args);
				surfaceOffset = Point.Subtract(args.Location, new Size(mouseStart));
				Invalidate(true);
			}

			mouseHoverStartTime = DateTime.Now;
		}

		/// <summary>
		/// Move all receptors in this membrane and recurse into child membranes to move those receptors as well.
		/// </summary>
		protected bool MoveReceptors(IMembrane m, Point offset)
		{
			bool moved = false;

			m.Receptors.ForEach(r =>
			{
				// System receptors aren't moved.  Their hidden.
				if (!r.Instance.IsHidden)
				{
					Point curPos = receptorLocation[r];
					receptorLocation[r] = Point.Add(curPos, new Size(offset));
					moved = true;
				}
			});

			((Membrane)m).Membranes.ForEach(inner => moved |= MoveReceptors(inner, offset));

			return moved;
		}

		/// <summary>
		/// Move the membrane and recurse into children to move them as well.
		/// </summary>
		protected void MoveMembranes(IMembrane m, Point offset)
		{
			if (membraneLocation.ContainsKey(m))
			{
				Point p = membraneLocation[m].Center;
				membraneLocation[m].Center = Point.Add(p, new Size(offset));
			}

			m.Membranes.ForEach(c => MoveMembranes(c, offset));
		}

		/// <summary>
		/// Recursively remove this membrane and all child membranes from the MembraneLocation map.
		/// </summary>
		/// <param name="m"></param>
		protected void RemoveMembranes(IMembrane m)
		{
			m.Membranes.ForEach(c => RemoveMembranes(c));
			membraneLocation.Remove(m);
		}

		/// <summary>
		/// Returns true if the membrane or its children have receptors.
		/// </summary>
		protected bool MembraneOrChildrenHaveReceptors(IMembrane m)
		{
			bool ret = m.Receptors.Count > 0;

			// stop if we have any membrane containing receptors.
			if (!ret)
			{
				m.Membranes.ForEach(c => ret |= MembraneOrChildrenHaveReceptors(c));
			}

			return ret;
		}

		protected bool HorizontalShakeTest(Point offset)
		{
			bool ret = false;
			TimeSpan ts = DateTime.Now - shakeStart;

			// Test only if dx is > dy
			if (Math.Abs(offset.X) > Math.Abs(offset.Y))
			{
				// If no movement for 1/2 second, then reset.
				if ((offset.X == 0) && ts.TotalMilliseconds > 500)
				{
					shakeStart = DateTime.Now;
					shakeCount = 0;
					shakeCurrentDirection = 0;
				}
				else
				{
					// Or moving in the same direction, reset again.
					if (Math.Sign(offset.X) == Math.Sign(shakeCurrentDirection) && ts.TotalMilliseconds > 500)
					{
						shakeStart = DateTime.Now;
						shakeCount = 0;
						shakeCurrentDirection = offset.X;
					}
					else if (Math.Sign(offset.X) != Math.Sign(shakeCurrentDirection) && ts.TotalMilliseconds < 500)
					{
						// Changing direction in under 500ms.  Increment the shake counter and reset the timer.
						shakeStart = DateTime.Now;
						++shakeCount;
						shakeCurrentDirection = offset.X;

						if (shakeCount >= 10)
						{
							// Success.  We have detected left-right shaking.
							ret = true;
						}
					}
					else if (ts.TotalMilliseconds > 500)
					{
						// Same direction for more than 500ms, so reset again.
						shakeStart = DateTime.Now;
						shakeCount = 0;
						shakeCurrentDirection = offset.X;
					}
				}
			}

			return ret;
		}

		protected bool VerticalShakeTest(Point offset)
		{
			bool ret = false;
			TimeSpan ts = DateTime.Now - shakeStart;

			// Test only if dy > dx
			if (Math.Abs(offset.Y) > Math.Abs(offset.X))
			{
				// If no movement for 1/2 second, then reset.
				if ((offset.Y == 0) && ts.TotalMilliseconds > 500)
				{
					shakeStart = DateTime.Now;
					shakeCount = 0;
					shakeCurrentDirection = 0;
				}
				else
				{
					// Or moving in the same direction, reset again.
					if (Math.Sign(offset.Y) == Math.Sign(shakeCurrentDirection) && ts.TotalMilliseconds > 500)
					{
						shakeStart = DateTime.Now;
						shakeCount = 0;
						shakeCurrentDirection = offset.Y;
					}
					else if (Math.Sign(offset.Y) != Math.Sign(shakeCurrentDirection) && ts.TotalMilliseconds < 500)
					{
						// Changing direction in under 500ms.  Increment the shake counter and reset the timer.
						shakeStart = DateTime.Now;
						++shakeCount;
						shakeCurrentDirection = offset.Y;

						if (shakeCount >= 10)
						{
							// Success.  We have detected left-right shaking.
							ret = true;
						}
					}
					else if (ts.TotalMilliseconds > 500)
					{
						// Same direction for more than 500ms, so reset again.
						shakeStart = DateTime.Now;
						shakeCount = 0;
						shakeCurrentDirection = offset.Y;
					}
				}
			}

			return ret;
		}

		protected void MouseEnterEvent(object sender, EventArgs args)
		{
			// We need to set focus to the control otherwise we don't get mouse wheel events.
			Focus();
		}

		protected void MouseWheelEvent(object sender, MouseEventArgs args)
		{
			Point testPoint = NegativeSurfaceOffsetAdjust(args.Location);
			var hoverReceptors = receptorLocation.Where(kvp => (new Rectangle(Point.Subtract(kvp.Value, ReceptorHalfSize), ReceptorSize)).Contains(testPoint));
			IReceptor hoverReceptor = null;

			if (hoverReceptors.Count() > 0)
			{
				hoverReceptor = hoverReceptors.First().Key;
			}

			if (hoverReceptor != null)
			{
				int spin = args.Delta / 120;			// Where does this constant come from?
				CarouselState cstate;

				if (carousels.TryGetValue(hoverReceptor, out cstate))
				{
					cstate.Offset += spin;
					Invalidate(true);
				}
			}
		}

		protected void MouseDoubleClickEvent(object sender, MouseEventArgs args)
		{
			Point p = args.Location;			// Mouse position
			// view the active image?
			// The selected image location is already adjusted for the surface offset.
			bool match = TestCarouselActiveImageDoubleClick(p);

			if (!match)
			{
				// Select image metadata?
				// The selected image location is already adjusted for the surface offset.
				match = TestImageMetadataDoubleClick(p);
			}

			if (!match)
			{
				// Enable/disable receptor?
				match = TestReceptorDoubleClick(NegativeSurfaceOffsetAdjust(p));
			}

			if (!match)
			{
				match = TestMembraneDoubleClick(NegativeSurfaceOffsetAdjust(p));
			}
		}

		protected bool TestCarouselActiveImageDoubleClick(Point p)
		{
			bool match = false;

			// Get the carousel state for the carousel with the active image that the user clicked on.
			var carousel = carousels.FirstOrDefault(kvp => kvp.Value.ActiveImageLocation.Contains(p));
			IReceptor r = carousel.Key;
			CarouselState cstate = carousel.Value;

			// If this is actually a carousel image:
			if (cstate != null)
			{
				string imageFile = cstate.ActiveImageFilename;
				ISemanticTypeStruct protocol = Program.SemanticTypeSystem.GetSemanticTypeStruct("ViewImage");
				dynamic signal = Program.SemanticTypeSystem.Create("ViewImage");
				signal.ImageFilename.Filename = imageFile.Surrounding("-thumbnail");

				IMembrane m = Program.Skin.GetMembraneContaining(r);
				m.CreateCarrier(r.Instance, protocol, signal);
				match = true;
			}

			return match;
		}

		protected bool TestImageMetadataDoubleClick(Point p)
		{
			ISemanticTypeSystem sts = Program.SemanticTypeSystem;

			foreach(var kvp in carousels)
				{
					Rectangle imgArea = kvp.Value.ActiveImageLocation;
					int imgidx = kvp.Value.ActiveImageIndex;
					int idx = -1;

					foreach(var meta in kvp.Value.Images[imgidx].MetadataPackets)
					{
						++idx;
						Rectangle metaRect = new Rectangle(imgArea.Left, imgArea.Bottom + 10 + (MetadataHeight * idx), imgArea.Width, MetadataHeight);
									
						if (metaRect.Contains(p))
						{
							// This is the metadata the user clicked on.
							// Now check if it's semantic data.  In all cases, this should be true, right?
							if (!String.IsNullOrEmpty(meta.ProtocolName))
							{
								// The implementing type is a semantic type requiring a drill into?
								// TODO: This actually needs to be recursive, drilling down to the ST that is a Native Type. 
								// See the implemntation in SemanticElement GetValue or SetValue.  We should add a method to the SE
								// to get us the SE that implements the NT!
								if (sts.GetSemanticTypeStruct(meta.Name).SemanticElements.Exists(st => st.Name == meta.PropertyName))
								{
									// Yes it is.  Emit a carrier with with protocol and signal.
									string implementingPropertyName = sts.GetSemanticTypeStruct(meta.ProtocolName).SemanticElements.Single(e => e.Name == meta.PropertyName).GetImplementingName(sts);
									ISemanticTypeStruct protocol = Program.SemanticTypeSystem.GetSemanticTypeStruct(meta.PropertyName);
									dynamic signal = Program.SemanticTypeSystem.Create(meta.PropertyName);
									protocol.AllTypes.Single(e => e.Name == implementingPropertyName).SetValue(Program.SemanticTypeSystem, signal, meta.Value);
									IReceptor r = kvp.Key;
									GetReceptorMembrane(r).CreateCarrier(r.Instance, protocol, signal);
										
									// Ugh, I hate doing this, but it's a lot easier to just exit all these nests.
									return true;
								}
								else if (sts.GetSemanticTypeStruct(meta.Name).NativeTypes.Exists(st => st.Name == meta.PropertyName))
								{
									// No, it's just a native type.
									ISemanticTypeStruct protocol = Program.SemanticTypeSystem.GetSemanticTypeStruct(meta.ProtocolName);
									dynamic signal = Program.SemanticTypeSystem.Create(meta.ProtocolName);
									sts.GetSemanticTypeStruct(meta.ProtocolName).NativeTypes.Single(st => st.Name == meta.PropertyName).SetValue(Program.SemanticTypeSystem, signal, meta.Value);
									IReceptor r = kvp.Key;
									GetReceptorMembrane(r).CreateCarrier(r.Instance, protocol, signal);

									// Ugh, I hate doing this, but it's a lot easier to just exit all these nests.
									return true;
								}
								// else: we don't have anythin we can do with this.
							}
						}
					}
				}

			return false;
		}

		/// <summary>
		/// Enable or disable the receptor being double-clicked.
		/// </summary>
		protected bool TestReceptorDoubleClick(Point p)
		{
			bool match = false;

			foreach (var kvp in receptorLocation)
			{
				Point rp = kvp.Value;
				rp.Offset(-ReceptorSize.Width / 2, -ReceptorSize.Height / 2);
				Rectangle r = new Rectangle(rp, ReceptorSize);

				if (r.Contains(p))
				{
					match = true;
					IReceptor receptor = kvp.Key;

					if (receptor.Instance.ConfigurationUI != null)
					{
						MycroParser mp = new MycroParser();
						Form form = mp.Load<Form>(receptor.Instance.ConfigurationUI, this);
						form.Tag = new ConfigurationInfo() { Receptor = receptor, Parser = mp };
						PopulateControls(receptor, mp);
						form.ShowDialog();
					}
					else
					{
						receptor.Enabled ^= true;
						Invalidate(true);
					}

					break;
				}
			}

			return match;
		}

		/// <summary>
		/// Populate controls mapped by the PropertyControlMap from the values in the receptor, acquired by reflection.
		/// </summary>
		protected void PopulateControls(IReceptor r, MycroParser mp)
		{
			foreach (PropertyControlEntry pce in ((PropertyControlMap)mp.ObjectCollection["ControlMap"]).Entries)		// TODO: magic name.
			{
				PropertyInfo piReceptor = r.Instance.GetType().GetProperty(pce.PropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				object val = piReceptor.GetValue(r.Instance);
				object control = mp.ObjectCollection[pce.ControlName];
				PropertyInfo piControl = control.GetType().GetProperty(pce.ControlPropertyName, BindingFlags.Public | BindingFlags.Instance);
				object convertedVal = Converter.Convert(val, piControl.PropertyType);
				piControl.SetValue(control, convertedVal);
			}

			// Special handling for "enabled."
			// TODO: Fix this by moving Enabled into IReceptorInstance and BaseReceptor
			object ckEnabled;
			if (mp.ObjectCollection.TryGetValue("ckEnabled", out ckEnabled))
			{
				((CheckBox)ckEnabled).Checked = r.Enabled;
			}
		}

		/// <summary>
		/// Save the data in the configuration UI form back to the receptor.
		/// </summary>
		protected void SaveValues(IReceptorInstance r, MycroParser mp)
		{
			foreach (PropertyControlEntry pce in ((PropertyControlMap)mp.ObjectCollection["ControlMap"]).Entries)		// TODO: magic name.
			{
				PropertyInfo piReceptor = r.GetType().GetProperty(pce.PropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				object control = mp.ObjectCollection[pce.ControlName];
				PropertyInfo piControl = control.GetType().GetProperty(pce.ControlPropertyName, BindingFlags.Public | BindingFlags.Instance);
				object val = piControl.GetValue(control);
				object convertedVal = Converter.Convert(val, piReceptor.PropertyType);
				piReceptor.SetValue(r, convertedVal);
			}
		}

		protected void OnReceptorConfigOK(object sender, EventArgs args)
		{
			Form form = (Form)((Control)sender).Parent;
			ConfigurationInfo ci = (ConfigurationInfo)form.Tag;
			SaveValues(ci.Receptor.Instance, ci.Parser);
			// Notify instance that the configuration has been updated.
			ci.Receptor.Instance.UserConfigurationUpdated();

			// Special handling for "enabled."
			// TODO: Fix this by moving Enabled into IReceptorInstance and BaseReceptor
			object ckEnabled;
			if (ci.Parser.ObjectCollection.TryGetValue("ckEnabled", out ckEnabled))
			{
				ci.Receptor.Enabled = ((CheckBox)ckEnabled).Checked;
				Invalidate(true);
			}

			form.Close();
		}

		protected void OnReceptorConfigCancel(object sender, EventArgs args)
		{
			((Form)((Control)sender).Parent).Close();
		}

		protected bool TestMembraneDoubleClick(Point p)
		{
			bool match = false;

			// Double clicking on the surface outside of any membrane automatically selects the skin.
			IMembrane membrane = FindInnermostSelectedMembrane(p, Program.Skin, false);

			// We ignore double-clicking on the skin.
			if (membrane != null)
			{
				// The grid we present to the user should look like this:
				// Membrane: [name]
				// Emits (outgoing):
				// [emitted protocol name] [permeable Y/N] (outgoing)
				// ...
				// Awaits (incoming):
				// [awaited protocol name] [permable Y/N] (incoming)

				// TODO: Should the membrane's permeability list include protocols not currently part of its receptor list?
				// At the moment, no.

				membraneBeingConfigured = membrane;
				Form form = MycroParser.InstantiateFromFile<Form>("membranePermeability.xml", null);

				// Create a data table here.  There's probably a better place to do this.
				// TODO: The first two columns are suppposed to be read-only.
				DataTable dt = new DataTable();
				dt.Columns.Add(new DataColumn("Protocol", typeof(string)));	
				dt.Columns.Add(new DataColumn("Direction", typeof(string)));
				dt.Columns.Add(new DataColumn("Permeable", typeof(bool)));
				membrane.UpdatePermeability();			// Get the current permeability, based on active child "outs" and active parent "ins".
				membrane.ProtocolPermeability.ForEach(kvp =>
					{
						DataRow row = dt.NewRow();
						row[0] = kvp.Key.Protocol;
						row[1] = kvp.Key.Direction;
						row[2] = kvp.Value.Permeable;
						dt.Rows.Add(row);
					});


				// Setup the data source.
				DataView dv = new DataView(dt);
				((DataGridView)form.Controls[0]).DataSource = dv;
				form.FormClosing += OnPermeabilityFormClosing;
				
				form.ShowDialog();
			}

			return match;
		}

		void OnPermeabilityFormClosing(object sender, FormClosingEventArgs e)
		{
			Form form = (Form)sender;
			DataGridView dgv = (DataGridView)form.Controls[0];
			dgv.EndEdit();
			DataView dv = (DataView)dgv.DataSource;
			dv.Table.AcceptChanges();

			// Save any changes the user made to permeability.
			dv.ForEach(row =>
				{
					string protocol = (string)row[0];
					PermeabilityDirection pd = ((string)row[1]).ToEnum<PermeabilityDirection>();
					PermeabilityKey pk = new PermeabilityKey() { Protocol = protocol, Direction = pd };
					bool permeable = (bool)row[2];
					membraneBeingConfigured.ProtocolPermeability[pk].Permeable = permeable;
				});

			CreateReceptorConnections();

			// Check queued receptors in all membranes.
			membraneLocation.Keys.ForEach(m => m.ProcessQueuedCarriers());
			// The skin, not part of this collection, needs its queued carriers checked as well.
			Program.Skin.ProcessQueuedCarriers();

			Invalidate(true);
		}

		/// <summary>
		/// The .NET MouseHoverEvent is f*cked.  It will not re-trigger until the mouse moves outside of the control.
		/// Even that seems problematic.  So we have a manual implementation.  Possibly wiring the event to the ViewControl 
		/// would work better for the pre-canned behavior, but the re-trigger is still an issue, it seems.
		/// </summary>
		protected void CheckMouseHover()
		{
			if ((DateTime.Now - mouseHoverStartTime).TotalMilliseconds > 500)
			{
				CarrierAnimationItem item = carrierAnimations.FirstOrDefault(a => a.CurrentRegion.Contains(NegativeSurfaceOffsetAdjust(mousePosition)));

				// Mouse is hovering over a carrier.
				if (item != null)
				{
					// Use the properties window to reveal the carrier contents.
					// The property grid is sort of stupid, so we'll put together an anonymous object for displaying the carrier protocol and signal.

					// new { Protocol = item.Carrier.Protocol.DeclTypeName, from t in Program.SemanticTypeSystem.GetSemanticTypeStruct(item.Carrier.Protocol.DeclTypeName).NativeTypes select new {Name = t.Name, Value = t.GetValue(item.Carrier.Signal)}};
					// var obj = (from t in Program.SemanticTypeSystem.GetSemanticTypeStruct(item.Carrier.Protocol.DeclTypeName).NativeTypes select new { Name = t.Name, Value = t.GetValue(item.Carrier.Signal) }).First();
					// var obj = new { Protocol = item.Carrier.Protocol.DeclTypeName, Signal = item.Carrier.Signal };
					// var obj = new CarrierProperty(item.Carrier.Protocol.DeclTypeName, item.Carrier.Signal);

					// There probably is a better way to do this:
					IDictionary dict = new Hashtable();
					dict.Add("Protocol", item.Carrier.Protocol.DeclTypeName);
					var kvpList = (from t in Program.SemanticTypeSystem.GetSemanticTypeStruct(item.Carrier.Protocol.DeclTypeName).NativeTypes select new { Name = t.Name, Value = t.GetValue(Program.SemanticTypeSystem, item.Carrier.Signal) }); // .ForEach((item) =
					kvpList.ForEach((kvp) => dict.Add(kvp.Name, kvp.Value));
					ApplicationController.PropertyGridController.ShowObject(new DictionaryPropertyGridAdapter(dict));
				}

				// Reset so that this test is not made again until the mouse is moved.
				mouseHoverStartTime = DateTime.MaxValue;
			}
		}

		/// <summary>
		/// Create the connections between receptors.
		/// </summary>
		protected void CreateReceptorConnections()
		{
			receptorConnections = new List<Connection>();
			Program.MasterReceptorConnectionList.Clear();

			// Iterate through all receptors.
			receptorLocation.ForEach(kvp1 =>
				{
					Membrane m1 = GetReceptorMembrane(kvp1.Key);

					// Get the emitted protocols of this receptor.
					kvp1.Key.Instance.GetEmittedProtocols().ForEach(prot1 =>
						{
							FindConnectionsWith(kvp1.Key, m1, prot1, kvp1.Value);
						});
				});

			// The "System" receptor can also be the originator of protocols.  Any protocol that the membrane's receptors receive protocols that aren't mapped to emitting receptors should be mapped to system.
			// However, because carriers are dropped into a particular membrane, we can't just assign the global system receptor to a bunch of receiving receptors, as these could be in different membranes.
			// the master receptor connection list is keyed by receptor instances in particular membranes, so this is ok.  The same cannot be said of the global system receptor.  Therefore, we have to
			// rely on the receptor system's protocol map for unmapped receivers.

			membraneLocation.Keys.ForEach(m => m.UpdateMasterConnectionList(Program.MasterReceptorConnectionList));
			Program.Skin.UpdateMasterConnectionList(Program.MasterReceptorConnectionList);
		}

		protected void FindConnectionsWith(IReceptor r, IMembrane m1, string prot1, Point rPoint, IMembrane source = null)
		{
			// Iterate through receptors with a second search.
			receptorLocation.ForEach(kvp2 =>
			{
				Membrane m2 = GetReceptorMembrane(kvp2.Key);

				// Receptors must be in the same membrane.
				if (m1 == m2)
				{
					// If any match the receive protocols of kvp2...
					if (kvp2.Key.Instance.GetReceiveProtocols().Select(rp=>rp.Protocol).Contains(prot1))
					{
						// Then these two receptors are connected.
						// P1 is always the emitter, P2 is always the receiver.
						Line l = new Line() { P1 = rPoint, P2 = kvp2.Value };

						// TODO: Yuck - there must be a better way of dealing with duplicates.
						Connection conn = new Connection() { Protocol = prot1, Line = l };
						if (!receptorConnections.Any(rc => rc.Line == l))
						{
							receptorConnections.Add(conn);
						}

						// Add this to the master connection list.
						// TODO: THIS SHOULD NOT BE COMPUTED IN THE VISUALIZER!!!!
						if (!Program.MasterReceptorConnectionList.ContainsKey(r))
						{
							Program.MasterReceptorConnectionList[r] = new List<IReceptor>();
						}

						// TODO: Yuck - there must be a better way of dealing with duplicates.
						if (!Program.MasterReceptorConnectionList[r].Contains(kvp2.Key))
						{
							Program.MasterReceptorConnectionList[r].Add(kvp2.Key);
						}
					}
				}
			});

			// Does the membrane allow this receptor protocol to move out?
			// Also, have we just come from the outer membrane, because we don't want to check it again as we
			// recurse inner membranes.
			PermeabilityKey pk = new PermeabilityKey() { Protocol = prot1, Direction = PermeabilityDirection.Out };

			if ((m1.ProtocolPermeability.ContainsKey(pk)) && (m1.ParentMembrane != source))
			{
				// Yes it does, check this membrane's receptors 
				// It must be an OUT direction, and it must be enabled.
				if (m1.ProtocolPermeability[pk].Permeable)
				{
					// Check outer mebranes, passing ourselves as the "inner source" (m1)
					FindConnectionsWith(r, m1.ParentMembrane, prot1, rPoint, m1);
				}
			}

			// Check inner membranes other than the outer membrane we are coming from.
			m1.Membranes.Where(m => m != source).ForEach(m =>
				{
					PermeabilityKey pk2 = new PermeabilityKey() { Protocol = prot1, Direction = PermeabilityDirection.In };

					// Does the inner membrane allow IN permeability?
					if (m.ProtocolPermeability.ContainsKey(pk2))
					{
						// Yes it does, check this membrane's receptors 
						// It must be an OUT direction, and it must be enabled.
						if (m.ProtocolPermeability[pk2].Permeable)
						{
							// Check the inner membrane.
							FindConnectionsWith(r, m, prot1, rPoint, m1);
						}
					}
				});

		}

		/// <summary>
		/// Return the membrane containing the receptor.
		/// </summary>
		protected Membrane GetReceptorMembrane(IReceptor r)
		{
			// Receptors always belong to membranes, and a receptor can never belong to more than one membrane.
			Membrane m = (Membrane)membraneLocation.Keys.Where(mTest => mTest.Receptors.Contains(r)).SingleOrDefault();

			if (m == null)
			{
				// TODO: Fix this so that the skin membrane is in the membrane location?
				m = Program.Skin;
			}

			return m;
		}

		/// <summary>
		/// Get the center and count of all non-hidden receptors in this membrane and all the child membranes.
		/// </summary>
		protected void GetCenter(IMembrane m, ref int cx, ref int cy, ref int count)
		{
			// Can't use ref'd variables in lambda expressions.
			foreach (IReceptor r in m.Receptors)
			{
				// System receptors are hidden.
				if (!r.Instance.IsHidden)
				{
					Point p = receptorLocation[r];
					cx += p.X;
					cy += p.Y;
					++count;
				}
			}

			// Recurse into child membranes.
			foreach (Membrane inner in m.Membranes)
			{
				GetCenter(inner, ref cx, ref cy, ref count);
			}
		}

		/// <summary>
		/// Get the radius of the membrane determined by the position of non-hidden receptors in this membrane
		/// and all child membranes.
		/// </summary>
		protected void GetMaxRadius(IMembrane m, int cx, int cy, ref double radius)
		{
			foreach(Receptor r in m.Receptors)
			{
				// System receptors are hidden.
				if (!r.Instance.IsHidden)
				{
					Point p = receptorLocation[r];
					double dx = p.X - cx;
					double dy = p.Y - cy;
					double dist = Math.Sqrt(dx * dx + dy * dy);

					if (dist > radius)
					{
						radius = dist;
					}
				}
			}

			// Recurse into child membranes.
			foreach (Membrane inner in m.Membranes)
			{
				GetMaxRadius(inner, cx, cy, ref radius);
			}
		}

		/// <summary>
		/// Recalculate the radii of the membranes.
		/// We used to remove membranes that did not contain any receptors, however
		/// this is a distinct possibility in how the user might want to work, by 
		/// creating multiple membrane layers first, without any receptors, or simply
		/// by creating membranes that are empty at the moment.
		/// </summary>
		protected void RecalcMembranes()
		{
			if (showMembranes)
			{
#if REMOVE_EMPTY_MEMBRANES
				List<IMembrane> toRemove = new List<IMembrane>();
#endif
				Dictionary<IMembrane, Circle> updates = new Dictionary<IMembrane, Circle>();

				// Get the center of all receptors within a membrane.
				// Can't use ref'd variables in lambda expressions.
				foreach (KeyValuePair<IMembrane, Circle> kvp in membraneLocation)
				{
					IMembrane m = kvp.Key;
#if REMOVE_EMPTY_MEMBRANES
					// Membrane must have receptors and can't be the root (skin) membrane.
					if ((m.Receptors.Count > 0) && (m.ParentMembrane != null))
#else
					// Skin membrane is not rendered.
					if (m.ParentMembrane != null)
#endif
					{
						int cx = 0, cy = 0, count = 0;

						// Much easier than using aggregate with Point structures.
						// Recurse into inner membranes as well.
						GetCenter(m, ref cx, ref cy, ref count);

						if (count != 0)
						{
							// This membrane still has receptors:
							cx /= count;
							cy /= count;

							// Get radius by finding the most distant receptor.
							double radius = 0;
							GetMaxRadius(m, cx, cy, ref radius);

							// Add a factor to the radius
							radius += 50;

							// If this membrane has child membranes, add a factor such that, if the outer membrane
							// has no receptors, it still renders visually as a bit bigger.
							if (m.Membranes.Count > 0)
							{
								radius += 50;
							}

							// Can't even modify the value of a collection being iterated!
							updates[m] = new Circle() { Center = new Point(cx, cy), Radius = (int)radius };
						}
						else
						{
							// The membrane has no receptors, so just draw the membrane with a fixed radius at 
							// whatever it's last position was.
							double radius = 50;

							// If this membrane has child membranes, add a factor such that, if the outer membrane
							// has no receptors, it still renders visually as a bit bigger.
							if (m.Membranes.Count > 0)
							{
								radius += 50;
							}

							updates[m] = new Circle() { Center = kvp.Value.Center, Radius = (int)radius };
						}
					}
#if REMOVE_EMPTY_MEMBRANES
					else if (m.ParentMembrane != null) // Obviously, don't remove the Skin membrane (not that it's in the list at the moment anyways.)
					{
						toRemove.Add(m);
					}
#endif
				}
#if REMOVE_EMPTY_MEMBRANES
				toRemove.ForEach(m => membraneLocation.Remove(m));
#endif
				updates.ForEach(kvp => membraneLocation[kvp.Key] = kvp.Value);
			}
		}

		protected void OnVisualizerPaint(object sender, PaintEventArgs e)
		{
			try
			{
				Control ctrl = (Control)sender;

				e.Graphics.FillRectangle(blackBrush, new Rectangle(Location, Size));
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

				if (showMembranes)
				{
					// Membranes are first
					membraneLocation.Values.ForEach(m =>
					{
						// Draw the surrounding membrane.
						GraphicsPath gp = new GraphicsPath();
						Rectangle r = CircleToBoundingRectangle(SurfaceOffsetAdjust(m.Center), m.Radius);
						r.Inflate(-20, -20);
						gp.AddEllipse(r);
						r.Inflate(20, 20);
						gp.AddEllipse(r);
						PathGradientBrush pgb = new PathGradientBrush(gp);
						// Read about blending here: http://msdn.microsoft.com/en-us/library/system.drawing.drawing2d.blend.aspx
						Blend b = new Blend();
						b.Factors = new float[] { 0, 1, 1 };
						b.Positions = new float[] { 0, .1f, 1 };
						pgb.Blend = b;
						// pgb.CenterPoint = m.Center;
						pgb.CenterColor = Color.Black;
						pgb.SurroundColors = new Color[] { Color.LightSlateGray };
						e.Graphics.FillPath(pgb, gp);
						pgb.Dispose();
						gp.Dispose();

						// Draw a nub at the center of the membrane.
						gp = new GraphicsPath();
						r = CircleToBoundingRectangle(SurfaceOffsetAdjust(m.Center), MembraneNubRadius);
						gp.AddEllipse(r);
						pgb = new PathGradientBrush(gp);
						pgb.CenterPoint = SurfaceOffsetAdjust(m.Center);
						pgb.CenterColor = Color.LightSlateGray;
						pgb.SurroundColors = new Color[] { Color.Black };
						e.Graphics.FillPath(pgb, gp);
						pgb.Dispose();
						gp.Dispose();


					});
				}

				e.Graphics.DrawImage(playButton, playButtonRect);
				e.Graphics.DrawImage(pauseButton, pauseButtonRect);

				// Draw connecting lines first, everything else is overlayed on top.

				receptorConnections.ForEach(conn =>
				{
					Line line = conn.Line;
					Pen pen;

					switch (conn.Protocol)
					{
						case "Text":
							pen = receptorLineColor2;
							break;
						case "HW_Player":
						case "HW_MoveTo":
							pen = receptorLineColor3;
							break;
						default:
							pen = receptorLineColor;
							break;
					}
					// Just a straight line:
					// e.Graphics.DrawLine(receptorLineColor, SurfaceOffsetAdjust(line.P1), SurfaceOffsetAdjust(line.P2));

					// The source starting point of the line should be placed on the edge of the receptor.
					double dx = line.P1.X - line.P2.X;
					double dy = line.P1.Y - line.P2.Y;
					double length = Math.Sqrt(dx * dx + dy * dy);

					// Don't bother if the receptors are nearly on top of each other.
					if (length > 2)
					{
						double ratio = 1.0 - (20 / length);
						Point start = new Point((int)(dx * ratio + line.P2.X), (int)(dy * ratio + line.P2.Y));

						double th = Math.Atan2(dy, dx);
						double th1 = th + 3 * Math.PI / 4;  // 45 degree offset
						double th2 = th + Math.PI / 4;  // 45 degree offset
						Point cp1 = new Point((int)(40 * Math.Cos(th1) + start.X), ((int)(40 * Math.Sin(th1) + start.Y)));
						Point cp2 = new Point((int)(40 * Math.Cos(th2) + line.P2.X), ((int)(40 * Math.Sin(th2) + line.P2.Y)));
						e.Graphics.DrawBezier(pen, SurfaceOffsetAdjust(start), SurfaceOffsetAdjust(cp1), SurfaceOffsetAdjust(cp2), SurfaceOffsetAdjust(line.P2));

						Point ctr = SurfaceOffsetAdjust(line.P2);
						// draw a small numb at the terminating point.
						e.Graphics.FillEllipse(new SolidBrush(pen.Color), new Rectangle(ctr.X - 3, ctr.Y - 3, 6, 6));
					}
				});

				// Draw receptors.

				receptorLocation.ForEach(kvp =>
					{
						// red for disabled receptors, green for enabled.
						Pen pen = kvp.Key.Enabled ? penColors[1] : penColors[0];
						Point p = SurfaceOffsetAdjust(kvp.Value);
						p.Offset(-ReceptorSize.Width / 2, -ReceptorSize.Height / 2);
						Point bottom = p;
						bottom.Offset(0, ReceptorSize.Height);
						Point bottomCenter = bottom;
						bottomCenter.Offset(ReceptorSize.Width / 2, 0);

						// Double plot because it looks better.
						e.Graphics.DrawEllipse(pen, new Rectangle(p, ReceptorSize));
						e.Graphics.DrawEllipse(pen, new Rectangle(p, ReceptorSize));

						// A double circle represents an edge receptor.
						if (kvp.Key.Instance.IsEdgeReceptor)
						{
							p.Offset(3, 3);		// GDI draws from the UL corner.
							Size s = Size.Subtract(ReceptorSize, new Size(6, 6));
							e.Graphics.DrawEllipse(pen, new Rectangle(p, s));
							e.Graphics.DrawEllipse(pen, new Rectangle(p, s));
						}

						// Name
						SizeF strSize = e.Graphics.MeasureString(kvp.Key.Instance.Name, font);
						Point center = Point.Subtract(bottomCenter, new Size((int)strSize.Width / 2, 0));
						e.Graphics.DrawString(kvp.Key.Name, font, whiteBrush, center);

						// Subname
						if (!String.IsNullOrEmpty(kvp.Key.Instance.Subname))
						{
							strSize = e.Graphics.MeasureString(kvp.Key.Instance.Subname, font);
							center = Point.Subtract(bottomCenter, new Size((int)strSize.Width / 2, -15));
							e.Graphics.DrawString(kvp.Key.Instance.Subname, font, whiteBrush, center);
						}
					});

				flyouts.ForEach(f =>
					{
						e.Graphics.DrawString(f.Text, font, whiteBrush, SurfaceOffsetAdjust(f.Location));
					});

				// Show carriers with targets.
				carrierAnimations.Where(q => q.Target != null).ForEach(a =>
					{
						// Get current target location in case user has moved it.
						Point p = receptorLocation.Single(kvp => kvp.Key.Instance == a.Target).Value;
						double dx = p.X - a.StartPosition.X;
						double dy = p.Y - a.StartPosition.Y;

						// Where are we on the curve?
						double q = Math.Sin((Math.PI / 2) * ((double)a.CurveIndex - CarrierTime / 2) / (CarrierTime / 2)) + 1;		// - PI/2 .. PI/2
						int idx = (int)(dx * q / 2.0);
						int idy = (int)(dy * q / 2.0);

						a.CurrentRegion = new Rectangle(a.StartPosition.X + idx - 5, a.StartPosition.Y + idy - 5, 10, 10);

						Point[] triangle = new Point[] 
					{ 
						SurfaceOffsetAdjust(new Point(a.StartPosition.X + idx, a.StartPosition.Y + idy)), 
						SurfaceOffsetAdjust(new Point(a.StartPosition.X + idx - 5, a.StartPosition.Y + idy + 5)), 
						SurfaceOffsetAdjust(new Point(a.StartPosition.X + idx + 5, a.StartPosition.Y + idy + 5)),
						SurfaceOffsetAdjust(new Point(a.StartPosition.X + idx, a.StartPosition.Y + idy)), 
					};

						e.Graphics.DrawLines(penColors[3], triangle);
					});

				// Show carriers without targets.
				carrierAnimations.Where(q => q.Target == null).ForEach(a =>
					{
						double dx = a.StartPosition.X;
						double dy = a.StartPosition.Y;

						// Where are we on the curve?
						double q = Math.Sin((Math.PI / 2) * ((double)a.CurveIndex - CarrierTime / 2) / (CarrierTime / 2)) + 1;		// - PI/2 .. PI/2
						int idx = (int)(dx * q / 2.0);
						int idy = (int)(dy * q / 2.0);

						a.CurrentRegion = new Rectangle(a.StartPosition.X + idx - 5, a.StartPosition.Y + idy - 5, 10, 10);

						Point[] triangle = new Point[] 
					{ 
						SurfaceOffsetAdjust(new Point(a.StartPosition.X + idx, a.StartPosition.Y + idy)), 
						SurfaceOffsetAdjust(new Point(a.StartPosition.X + idx - 5, a.StartPosition.Y + idy + 5)), 
						SurfaceOffsetAdjust(new Point(a.StartPosition.X + idx + 5, a.StartPosition.Y + idy + 5)),
						SurfaceOffsetAdjust(new Point(a.StartPosition.X + idx, a.StartPosition.Y + idy)), 
					};

						e.Graphics.DrawLines(penColors[3], triangle);
					});
// Rework Idea:
/*
				carousels.ForEach(kvp =>
				{
					Point p = receptorLocation[kvp.Key];
					int imagesCount = kvp.Value.Images.Count;
					int offset = kvp.Value.Offset;
					int idx0 = 0;
					//int sizeZ = 40;
					//int idxReal = 0;
					Image img = null;
					//Point ip;
					//double theta = 0;
					//double dx = 0;
					//double dy = 0;

					// The images in the carousel should range from (relative to the receptor center):
					// -80 ... +80  (see sizeZ, which is set to 160.)
					// of course, on the left of the center image, this needs to be the right-edge position.
					// and on the right of the cemter image, this needs to be the left-edge position.
					// It would be easier to work with the center of the images on the carousel, which
					// should be some % of the center width (160), decreasing as we move up the carousel, 
					// to provide a 3D effect.
					// If we assume an image width of 160 for the two edge images, then our offsets from center
					// will be +/- 160.
					// We can therefore compute the starting and ending angles assuming a maximum height of 100
					// angle = acos(160/100)
					// Of course, these angles need to be adjusted because the are in the 3rd and 4th quadrants:
					// (in degrees):
					//     starting angle = 270 - startangle
					//     ending angle = 270 + startangle
					// and we iterate from starting angle backwards to the ending angle.
					// steps = (starting angle + (360 - ending angle)) / num images

					double deg270 = 2 * Math.PI * 3 / 4;
					double angle = Math.Atan(100 / 160);
					double startingAngle = deg270 - angle;
					double endingAngle = deg270 + angle;
					double range = startingAngle + 2 * Math.PI - endingAngle;
					double step = range / imagesCount;
					double imageSizeStep = Math.PI / imagesCount;			// 0 to 180 degrees

					kvp.Value.Images.ForEachWithIndex((imeta, idx) =>
					{
						Point ip = p;
						int idxReal = Math.Abs((idx + offset) % imagesCount);
						img = kvp.Value.Images[idxReal].Image;
						double theta = startingAngle - step * idx;
						double dx = 160 * Math.Cos(theta);
						double dy = -100 * Math.Sin(theta);
						ip.Offset((int)dx, (int)dy);

						if (idxReal == 0)
						{
							// This is the "selected" image.
							// We also don't want to display this image in the carousel, otherwise it appears twice.
							idx0 = idx;
						}
						else
						{
							// from nearly full width as we go around the arc to where we have the smallest width at the top of the arc, then back again.
							int sizeZ = (int)((160 - 10) * (1.0 - (0.25 + Math.Sin(imageSizeStep * idx) * 3 / 4)));
							Rectangle rect = new Rectangle(new Point(ip.X - sizeZ/2 , ip.Y), new Size(sizeZ, sizeZ * img.Height / img.Width));
							e.Graphics.DrawImage(img, rect);
							e.Graphics.DrawString(idx.ToString(), font, whiteBrush, rect);
						}
					});

					// Draw idx0 last so it appears on top.
					// The image is centered below the receptor.
					//idxReal = (idx0 + offset) % imagesCount;
					//ip = p;
					//theta = (Math.PI * 0.56) + 2 * Math.PI * idxReal / imagesCount;
					//dx = 200 * Math.Cos(theta);
					//dy = 100 * Math.Sin(theta);
					//ip.Offset((int)dx, (int)dy);
					img = kvp.Value.Images[idx0].Image;
					//sizeZ = 160; //  (idxReal == 0) ? 160 : 10;
					//var posY = ip.Y + 20;
					//var posX = ip.X - 40; 

					int sizeZ2 = 160;
					Point rp = receptorLocation[kvp.Key];
					rp.Offset(-sizeZ2 / 2, 172);

					Rectangle location = new Rectangle(rp, new Size(sizeZ2, sizeZ2 * img.Height / img.Width));
					e.Graphics.DrawImage(img, location);
					kvp.Value.ActiveImageFilename = img.Tag.ToString();
					kvp.Value.ActiveImageLocation = location;
					kvp.Value.ActiveImageIndex = idx0;

					int y = location.Bottom + 10;

					kvp.Value.Images[idx0].MetadataPackets.ForEach(meta =>
					{
						Rectangle region = new Rectangle(location.X, y, location.Width, MetadataHeight);
						string data = meta.Name + ": " + meta.Value;
						e.Graphics.DrawString(data, font, whiteBrush, region);
						y += MetadataHeight;
					});
				});
*/

#if VIVEK
				carousels.ForEach(kvp =>
				{
					Point p = SurfaceOffsetAdjust(receptorLocation[kvp.Key]);
					int imagesCount = kvp.Value.Images.Count;
					int offset = kvp.Value.Offset;
					int idx0 = 0;
					int sizeZ = 40;
					int idxReal = 0;
					Image img = null;
					Point ip;
					double theta = 0;
					double dx = 0;
					double dy = 0;

					kvp.Value.Images.ForEachWithIndex((imeta, idx) =>
					{
						img = imeta.Image;
						ip = p;
						idxReal = (idx + offset) % imagesCount;
						theta = (Math.PI * 0.43) + 2 * Math.PI * idxReal / imagesCount;
						dx = 200 * Math.Cos(theta);
						dy = 100 * Math.Sin(theta);
						ip.Offset((int)dx, (int)dy);

						if (idxReal == 0)
						{
							idx0 = idx;
						}
						else
						{
							sizeZ += (90 / imagesCount);

							//e.Graphics.FillRectangle(new SolidBrush(Color.Yellow), ip.X-20, ip.Y-30, 5, 5); //markers
							if (imagesCount < 10)
								sizeZ = 75;

							e.Graphics.DrawImage(img, new Rectangle(new Point(ip.X - 20, ip.Y - 30 * img.Width / img.Height), new Size(sizeZ, sizeZ * img.Height / img.Width)));
						}

					});

					img = kvp.Value.Images[idx0].Image;
					int sizeZ2 = 160;
					Point rp = SurfaceOffsetAdjust(receptorLocation[kvp.Key]);
					rp.Offset(-sizeZ2 / 2, 100);		// 100 is some arbitrary vertical offset for testing.
					Rectangle location = new Rectangle(rp, new Size(sizeZ2, sizeZ2 * img.Height / img.Width));
					e.Graphics.DrawImage(img, location);

					kvp.Value.ActiveImageFilename = img.Tag.ToString();
					kvp.Value.ActiveImageLocation = location;
					kvp.Value.ActiveImageIndex = idx0;

					int y = location.Bottom + 10;

					// We use ForEachWithIndex to ensure the same ordering as when the user double-clicks on the metadata.
					kvp.Value.Images[idx0].MetadataPackets.ForEachWithIndex((meta, idx) =>
					{
						Rectangle region = new Rectangle(location.X, y, location.Width, MetadataHeight);
						string data = meta.Name + ": " + meta.Value;
						e.Graphics.DrawString(data, font, whiteBrush, region);
						y += MetadataHeight;
					});

				});

#endif
// Decent.
#if MINE
				carousels.ForEach(kvp =>
					{
						Point p = receptorLocation[kvp.Key];
						// p.Offset(-ReceptorSize.Width / 2, -ReceptorSize.Height / 2);
						int images = kvp.Value.Images.Count;
						int offset = kvp.Value.Offset;
						int idx0 = 0;

						kvp.Value.Images.ForEachWithIndex((imeta, idx) =>
						{
							Image img = imeta.Image;
							int idxReal = (idx + offset) % images;
							Point ip = p;
							double dx = 200 * Math.Cos((2 * Math.PI * 1 / 4) + 2 * Math.PI * idxReal / images);
							double dy = 100 * Math.Sin((2 * Math.PI * 1 / 4) + 2 * Math.PI * idxReal / images);
							ip.Offset((int)dx, (int)dy);
							int sizer = (int)(100 * (0.25 + ((1.0 + Math.Sin((2 * Math.PI * 1 / 4) + 2 * Math.PI * idxReal / images) / 2) * 3 / 4)));

							if (idxReal == 0)
							{
								idx0 = idx;
							}
							else
							{
								e.Graphics.DrawImage(img, new Rectangle(new Point(ip.X - 50, ip.Y - 50 * img.Height / img.Width), new Size(sizer, sizer * img.Height / img.Width)));
							}
						});

						{
							// Draw idx0 last so it appears on top.
							int idxReal = (idx0 + offset) % images;
							Point ip = p;
							double dx = 150 * Math.Cos((2 * Math.PI * 1 / 4) + 2 * Math.PI * idxReal / images);
							double dy = 150 * Math.Sin((2 * Math.PI * 1 / 4) + 2 * Math.PI * idxReal / images);
							ip.Offset((int)dx, (int)dy);
							int sizer = (idxReal == 0) ? 150 : 100;
							Image img = kvp.Value.Images[idx0].Image;
							Rectangle location = new Rectangle(new Point(ip.X - 75, ip.Y - 50 * img.Height / img.Width), new Size(sizer, sizer * img.Height / img.Width));
							e.Graphics.DrawImage(img, location);
							kvp.Value.ActiveImageFilename = img.Tag.ToString();
							kvp.Value.ActiveImageLocation = location;
							kvp.Value.ActiveImageIndex = idx0;

							int y = location.Bottom + 10;

							// We use ForEachWithIndex to ensure the same ordering as when the user double-clicks on the metadata.
							kvp.Value.Images[idx0].MetadataPackets.ForEachWithIndex((meta, idx) =>
								{
									Rectangle region = new Rectangle(location.X, y, location.Width, MetadataHeight);
									string data = meta.Name + ": " + meta.Value;
									e.Graphics.DrawString(data, font, whiteBrush, region);
									y += MetadataHeight;
								});
						}
					});
#endif  

				if (rubberBand)
				{
					Rectangle r = Rectangle.FromLTRB(Math.Min(mouseStart.X, mousePosition.X), Math.Min(mouseStart.Y, mousePosition.Y), Math.Max(mouseStart.X, mousePosition.X), Math.Max(mouseStart.Y, mousePosition.Y));
					e.Graphics.DrawRectangle(whitePen, r);
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debugger.Break();
			}
		}

		protected Rectangle CircleToBoundingRectangle(Point ctr, int radius)
		{
			return new Rectangle(ctr.X - radius, ctr.Y - radius, radius * 2, radius * 2);
		}

		/// <summary>
		/// Returns a point adjusted (adding) for the surface offset.
		/// </summary>
		public Point SurfaceOffsetAdjust(Point src)
		{
			Point p = src;
			p.Offset(surfaceOffset);

			return p;
		}

		/// <summary>
		/// Returns a point adjusted for the surface offset by subtracting the current surface offset.
		/// </summary>
		public Point NegativeSurfaceOffsetAdjust(Point src)
		{
			Point p = src;
			p.Offset(-surfaceOffset.X, -surfaceOffset.Y);

			return p;
		}
	}
}

