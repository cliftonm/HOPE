using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using Clifton.ExtensionMethods;
using Clifton.MycroParser;
using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.Receptor.Interfaces
{
	/// <summary>
	/// Acts like a semaphore.  Decrements a counter and when the count reaches zero, executes the action.
	/// Used to synchronize multiple responses such that, only when all the responses are received, will the 
	/// next step in a workflow be executed.
	/// </summary>
	public class Gate
	{
		public int Count { get; set; }
		public Action NextStep { get; set; }

		public void Decrement()
		{
			if (--Count == 0)
			{
				NextStep();
			}
		}
	}

	public class CompositeGate
	{
		public Action NextStep { get; set; }
		public Dictionary<string, Gate> Gates;

		public CompositeGate()
		{
			Gates = new Dictionary<string, Gate>();
		}
	}

	/// <summary>
	/// Useful to derive from this receptor when not needing to implement every single property / event handler.
	/// </summary>
	public abstract class BaseReceptor : IReceptorInstance
	{
		public event EventHandler<EventArgs> ReceiveProtocolsChanged;
		public event EventHandler<EventArgs> EmitProtocolsChanged;

		public abstract string Name { get; }
		public virtual string Subname { get { return String.Empty; } }
		public virtual bool IsEdgeReceptor { get { return false; } }
		public virtual bool IsHidden { get { return false; } }

		/// <summary>
		/// Allows a receptor to specify a MyXaml (MycroXaml) form UI for configuring the receptor.
		/// The default (null) means "no configuration UI."
		/// </summary>
		public virtual string ConfigurationUI { get { return null; } }

		protected List<ReceiveQualifier> receiveProtocols;
		protected List<string> emitProtocols;
		protected Dictionary<string, Gate> gates;
		protected Dictionary<string, CompositeGate> compositeGates;

		protected bool receptorInitialized;
		protected bool systemInitialized;

		public virtual IReceptorSystem ReceptorSystem
		{
			get { return rsys; }
			set { rsys = value; }
		}

		protected IReceptorSystem rsys;

		public BaseReceptor(IReceptorSystem rsys)
		{
			this.rsys = rsys;
			receiveProtocols = new List<ReceiveQualifier>();
			emitProtocols = new List<string>();
			gates = new Dictionary<string, Gate>();
			compositeGates = new Dictionary<string, CompositeGate>();
		}

		/// <summary>
		/// Called when the receptor initialization has completed.
		/// </summary>
		public virtual void Initialize()
		{
			receptorInitialized = true;
		}

		/// <summary>
		/// Called when the system shuts down, allowing receptor instances to dispose of unmanaged resources and 
		/// perform other cleanup.
		/// Also called when a new applet is loaded.
		/// </summary>
		public virtual void Terminate()
		{
		}

		public virtual List<ReceiveQualifier> GetReceiveProtocols()
		{
			return receiveProtocols;
		}

		public virtual List<string> GetEmittedProtocols()
		{
			return emitProtocols;
		}

		/// <summary>
		/// If not overridden, will invoke the action associated with the receive protocol that is qualified by the qualifier function.
		/// </summary>
		/// <param name="carrier"></param>
		public virtual void ProcessCarrier(ICarrier carrier)
		{
			ReceiveQualifier rq = receiveProtocols.Find(rp => rp.Protocol == carrier.Protocol.DeclTypeName && rp.Qualifier(carrier.Signal));
			rq.Action(carrier.Signal);
		}

		/// <summary>
		/// Called when the entire system has been initialized, after loading an applet.
		/// Called on an individual receptor when it is dragged and dropped onto an existing surface.
		/// </summary>
		public virtual void EndSystemInit()
		{
			systemInitialized = true;
		}

		/// <summary>
		/// Called when the user configurable items in a receptor instance have been updated by user or other action.
		/// </summary>
		public virtual void UserConfigurationUpdated()
		{
		}

		/// <summary>
		/// Register a single gate.  This will trigger the next step action when the count for this gate reaches 0.
		/// </summary>
		public void RegisterGate(string key, int count, Action nextStep)
		{
			gates[key] = new Gate() { Count = count, NextStep = nextStep };
		}

		/// <summary>
		/// Decrement a single gate.  This will trigger the next step action when the count for this gate reaches 0.
		/// </summary>
		public void DecrementGate(string key)
		{
			gates[key].Decrement();
		}

		/// <summary>
		/// Registers a composite gate.  When all the gates have decremented to 0, the composite gate action is performed.
		/// When any gate reaches 0, its specific action is performed first.
		/// </summary>
		public void RegisterCompositeGate(string key, Action nextStep)
		{
			compositeGates[key] = new CompositeGate() { NextStep = nextStep };
		}

		/// <summary>
		/// Register a specific gate that is a member of the composite gate.
		/// </summary>
		public void RegisterCompositeGateGate(string key, string gateKey, int count, Action nextStep)
		{
			compositeGates[key].Gates[gateKey] = new Gate() { Count = count, NextStep = nextStep };
		}

		/// <summary>
		/// Decrements the specified gate.  When all gates have decremented to 0, the composite action is also executed.
		/// </summary>
		public void DecrementCompositeGate(string key, string gateKey)
		{
			CompositeGate cg = compositeGates[key];
			cg.Gates[gateKey].Decrement();

			if (cg.Gates.Values.All(g => g.Count == 0))
			{
				cg.NextStep();
			}
		}

		public virtual void PrepopulateConfig(Clifton.MycroParser.MycroParser mp)
		{
		}

		/// <summary>
		/// Add an unqualified receive protocol.  Override ProcessCarrier to handle this protocol.
		/// </summary>
		protected virtual void AddReceiveProtocol(string p)
		{
			receiveProtocols.Add(new ReceiveQualifier(p));
			ReceiveProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		/// <summary>
		/// Add an unqualified receive protocol and execute the specified action when received.
		/// </summary>
		protected virtual void AddReceiveProtocol(string p, Action<dynamic> a)
		{
			receiveProtocols.Add(new ReceiveQualifier(p, a));
			ReceiveProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		/// <summary>
		/// Add a receive protocol that is qualified by a function.
		/// </summary>
		protected virtual void AddReceiveProtocol(string p, Func<dynamic, bool> q)
		{
			receiveProtocols.Add(new ReceiveQualifier(p, q));
			ReceiveProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		/// <summary>
		/// Add a receive protocol that is qualified by a function and specifies the action when qualified.
		/// </summary>
		protected virtual void AddReceiveProtocol(string p, Func<dynamic, bool> q, Action<dynamic> a)
		{
			receiveProtocols.Add(new ReceiveQualifier(p, q, a));
			ReceiveProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		/// <summary>
		/// Remove a specific receive protocol.
		/// </summary>
		protected virtual void RemoveReceiveProtocol(string p)
		{
			receiveProtocols.Remove(receiveProtocols.Single(rp=>rp.Protocol==p));
			ReceiveProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		/// <summary>
		/// Remove all receive protocols.
		/// </summary>
		protected virtual void RemoveReceiveProtocols()
		{
			receiveProtocols.Clear();
			ReceiveProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		/// <summary>
		/// Add protocol that this receptor emits.
		/// </summary>
		protected virtual void AddEmitProtocol(string p, bool processInternalSemanticElements = true)
		{
			if (!emitProtocols.Contains(p))
			{
				emitProtocols.Add(p);
				EmitProtocolsChanged.Fire(this, EventArgs.Empty);

				// Kludge to allow a receptor to specify that internal semantic elements of a protocol
				// should not be processed.  ThumbnailCreatorReceptor and ThumbnailViewerReceptor are 
				// good use cases for this, as otherwise an infinite loop will occur between the two,
				// as the creator also would emit the SE "ImageFilename" and the viewer emits "ImageFilename"
				// as part of the SE's ViewImage and GetImageMetadata 
				if (processInternalSemanticElements)
				{
					AddInternalSemanticElements(p);
				}
			}
		}

		/// <summary>
		/// Remove a specific emit protocol.
		/// </summary>
		protected virtual void RemoveEmitProtocol(string p)
		{
			emitProtocols.Remove(p);
			EmitProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		/// <summary>
		/// Remove all emit protocols.
		/// </summary>
		protected virtual void RemoveEmitProtocols()
		{
			emitProtocols.Clear();
			EmitProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		protected void CreateCarrier(string protocol, Action<dynamic> initializeSignal)
		{
			ISemanticTypeStruct outprotocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct(protocol);
			dynamic outsignal = rsys.SemanticTypeSystem.Create(protocol);
			initializeSignal(outsignal);
			rsys.CreateCarrier(this, outprotocol, outsignal);
		}

		protected void CreateCarrierIfReceiver(string protocol, Action<dynamic> initializeSignal)
		{
			ISemanticTypeStruct outprotocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct(protocol);
			dynamic outsignal = rsys.SemanticTypeSystem.Create(protocol);
			initializeSignal(outsignal);
			rsys.CreateCarrierIfReceiver(this, outprotocol, outsignal);
		}

		/// <summary>
		/// This is such a common call that receptors make, that we provide a helper function
		/// for it here in the BaseReceptor.  If you make this call, be sure to add an AddEmitProtocol("RequireTable")
		/// to your receptor so it gets wired up to the persistence receptor.
		/// </summary>
		/// <param name="tableName"></param>
		protected void RequireTable(string tableName)
		{
			CreateCarrierIfReceiver("RequireTable", signal =>
				{
					signal.TableName = tableName;
					signal.Schema = tableName;
				});
		}

		/// <summary>
		/// Instantiate a carrier of the specified protocol and passing in the signal initialization action.
		/// Often used for instantiating carriers passed as the Row parameter to the persistence receptor.
		/// </summary>
		protected ICarrier InstantiateCarrier(string protocol, Action<dynamic> initializeSignal)
		{
			ISemanticTypeStruct semStruct = rsys.SemanticTypeSystem.GetSemanticTypeStruct(protocol);
			dynamic signal = rsys.SemanticTypeSystem.Create(protocol);
			initializeSignal(signal);
			ICarrier rowCarrier = rsys.CreateInternalCarrier(semStruct, signal);

			return rowCarrier;
		}

		/// <summary>
		/// A helper method for no action.
		/// </summary>
		protected void NullAction()
		{
		}

		/// <summary>
		/// Emits the exception as a carrier, which can be viewed, logged, etc, by other receptors.
		/// </summary>
		protected void EmitException(Exception ex)
		{
			CreateCarrierIfReceiver("Exception", signal =>
			{
				signal.ReceptorName = Name;
				signal.Message = ex.Message;
			});
		}

		/// <summary>
		/// Emits the exception as a carrier, which can be viewed, logged, etc, by other receptors.
		/// </summary>
		protected void EmitException(string message)
		{
			CreateCarrierIfReceiver("Exception", signal =>
			{
				signal.ReceptorName = Name;
				signal.Message = message;
			});
		}

		/// <summary>
		/// This is an interesting function that looks at the internals of the protocol, and
		/// for every semantic element, it adds an emitter protocol for that type as well.
		/// When the carrier is actually created, additional carriers for internal semantic elements
		/// will also be created conditionally, if a receiver exists.
		/// This behavior is "exploratory" in that we may not always want this, but since this whole
		/// concept is so unique, there really is no "best practice" for this behavior.
		/// Also note that this method recurses!
		/// </summary>
		protected void AddInternalSemanticElements(string protocol)
		{
			ISemanticTypeStruct st = rsys.SemanticTypeSystem.GetSemanticTypeStruct(protocol);

			st.SemanticElements.ForEach(se =>
				{
					// Recurse, adding semantic elements that are part of the parent protocol.
					AddEmitProtocol(se.Name);
				});
		}

		protected virtual Tuple<Form, Clifton.MycroParser.MycroParser> InitializeViewer(string formName)
		{
			Clifton.MycroParser.MycroParser mp = new Clifton.MycroParser.MycroParser();
			XmlDocument doc = new XmlDocument();
			doc.Load(formName);
			mp.Load(doc, "Form", null);
			Form form = (Form)mp.Process();

			return new Tuple<Form, MycroParser.MycroParser>(form, mp);
		}
	}
}
