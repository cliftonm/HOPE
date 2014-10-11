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
		public virtual bool Enabled { get; set; }

		/// <summary>
		/// Allows a receptor to specify a MyXaml (MycroXaml) form UI for configuring the receptor.
		/// The default (null) means "no configuration UI."
		/// </summary>
		public virtual string ConfigurationUI { get { return null; } }
		
		/// <summary>
		/// Receptors set this value to provide a message to the user when a configuration error occurs.
		/// </summary>
		public string ConfigurationError { get; protected set; }

		protected List<ReceiveQualifier> receiveProtocols;
		protected List<EmittedProtocol> emitProtocols;
		protected Dictionary<string, bool> cachedReceiveProtocolConfig;
		protected Dictionary<string, bool> cachedEmitProtocolConfig;

		protected Dictionary<string, Gate> gates;
		protected Dictionary<string, CompositeGate> compositeGates;

		protected bool receptorInitialized;
		protected bool systemInitialized;
		protected string currentProtocol;			// Used in the exception handling message to indicate the protocol name being processed when the exception occurred.

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
			emitProtocols = new List<EmittedProtocol>();
			cachedEmitProtocolConfig = new Dictionary<string, bool>();
			cachedReceiveProtocolConfig = new Dictionary<string, bool>();
			gates = new Dictionary<string, Gate>();
			compositeGates = new Dictionary<string, CompositeGate>();
			Enabled = true;
		}

		/// <summary>
		/// We cache the persisted enable state of the emit protocols for receptors that register emit protocols after initialization.
		/// </summary>
		public void CacheEmitProtocol(string protocolName, bool enabled)
		{
			cachedEmitProtocolConfig[protocolName] = enabled;
		}

		/// <summary>
		/// We cache the persisted enable state of the receive protocols for receptors that register receive protocols after initialization.
		/// </summary>
		public void CacheReceiveProtocol(string protocolName, bool enabled)
		{
			cachedReceiveProtocolConfig[protocolName] = enabled;
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

		/// <summary>
		/// Return all receive protocols.
		/// </summary>
		public virtual List<ReceiveQualifier> GetReceiveProtocols()
		{
			return receiveProtocols;
		}

		/// <summary>
		/// Returns all emitted protocols.
		/// </summary>
		/// <returns></returns>
		public virtual List<EmittedProtocol> GetEmittedProtocols()
		{
			return emitProtocols;
		}

		/// <summary>
		/// Returns only enabled receive protocols.
		/// </summary>
		public virtual List<ReceiveQualifier> GetEnabledReceiveProtocols()
		{
			return receiveProtocols.Where(p => p.Enabled).ToList();
		}

		/// <summary>
		/// Returns only enabled emitted protocols.
		/// </summary>
		public virtual List<EmittedProtocol> GetEnabledEmittedProtocols()
		{
			return emitProtocols.Where(p => p.Enabled).ToList();
		}

		/// <summary>
		/// If not overridden, will invoke the action associated with the receive protocol that is qualified by the qualifier function.
		/// </summary>
		/// <param name="carrier"></param>
		public virtual void ProcessCarrier(ICarrier carrier)
		{
			currentProtocol = carrier.Protocol.DeclTypeName;		// Used in exception processing.
			ReceiveQualifier rq = receiveProtocols.Find(rp => rp.Protocol == carrier.Protocol.DeclTypeName && rp.Qualifier(carrier.Signal));

			try
			{
				rq.Action(carrier.Signal);
			}
			catch (Exception ex)
			{
				EmitException(ex);
			}
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
		public virtual bool UserConfigurationUpdated()
		{
			return true;
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
			bool cachedEnableState;
			cachedReceiveProtocolConfig.TryGetValue(p, out cachedEnableState).Else(() => cachedEnableState = true);
			receiveProtocols.Add(new ReceiveQualifier(p) { Enabled = cachedEnableState });
			ReceiveProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		/// <summary>
		/// Add an unqualified receive protocol and execute the specified action when received.
		/// </summary>
		protected virtual void AddReceiveProtocol(string p, Action<dynamic> a)
		{
			bool cachedEnableState;
			cachedReceiveProtocolConfig.TryGetValue(p, out cachedEnableState).Else(() => cachedEnableState = true);
			receiveProtocols.Add(new ReceiveQualifier(p, a) { Enabled = cachedEnableState });
			ReceiveProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		/// <summary>
		/// Add a receive protocol that is qualified by a function.
		/// </summary>
		protected virtual void AddReceiveProtocol(string p, Func<dynamic, bool> q)
		{
			bool cachedEnableState;
			cachedReceiveProtocolConfig.TryGetValue(p, out cachedEnableState).Else(() => cachedEnableState = true);
			receiveProtocols.Add(new ReceiveQualifier(p, q) { Enabled = cachedEnableState });
			ReceiveProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		/// <summary>
		/// Add a receive protocol that is qualified by a function and specifies the action when qualified.
		/// </summary>
		protected virtual void AddReceiveProtocol(string p, Func<dynamic, bool> q, Action<dynamic> a)
		{
			bool cachedEnableState;
			cachedReceiveProtocolConfig.TryGetValue(p, out cachedEnableState).Else(() => cachedEnableState = true);
			receiveProtocols.Add(new ReceiveQualifier(p, q, a) { Enabled = cachedEnableState });
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
		protected virtual void AddEmitProtocol(string protocolName, bool processInternalSemanticElements = true)
		{
			// We can encounter the same protocol when we recurse into the internal semantics, so ignore repeats.
			if (!emitProtocols.Exists(p=>p.Protocol==protocolName))
			{
				bool cachedEnableState;
				cachedEmitProtocolConfig.TryGetValue(protocolName, out cachedEnableState).Else(() => cachedEnableState = true);
				emitProtocols.Add(new EmittedProtocol() { Protocol = protocolName, Enabled=cachedEnableState });
				EmitProtocolsChanged.Fire(this, EventArgs.Empty);

				// Kludge to allow a receptor to specify that internal semantic elements of a protocol
				// should not be processed.  ThumbnailCreatorReceptor and ThumbnailViewerReceptor are 
				// good use cases for this, as otherwise an infinite loop will occur between the two,
				// as the creator also would emit the SE "ImageFilename" and the viewer emits "ImageFilename"
				// as part of the SE's ViewImage and GetImageMetadata 
				if (processInternalSemanticElements)
				{
					AddInternalSemanticElements(protocolName);
				}
			}
		}

		/// <summary>
		/// Remove a specific emit protocol.
		/// </summary>
		protected virtual void RemoveEmitProtocol(string protocolName)
		{
			emitProtocols.Remove(emitProtocols.Single(p => p.Protocol == protocolName));
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
			// Test is made for the benefit of unit testing, which doesn't necessarily instantiate this message.
			if (rsys.SemanticTypeSystem.VerifyProtocolExists("ExceptionMessage"))
			{
				CreateCarrierIfReceiver("ExceptionMessage", signal =>
				{
					signal.ReceptorName = Name;
					signal.MessageTime = DateTime.Now;
					signal.ProtocolName = currentProtocol;
					signal.TextMessage.Text.Value = ex.Message;
				});
			}
			else
			{
				throw ex;
			}
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
