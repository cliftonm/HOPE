using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Clifton.ExtensionMethods;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.Receptor
{
	/// <summary>
	/// Event args for a new receptor notification.
	/// </summary>
	public class ReceptorEventArgs : EventArgs
	{
		public IReceptor Receptor { get; protected set; }

		public ReceptorEventArgs(IReceptor receptor)
		{
			Receptor = receptor;
		}
	}

	/// <summary>
	/// Event args for a new carrier notification.
	/// </summary>
	public class NewCarrierEventArgs : EventArgs
	{
		public IReceptorInstance From { get; protected set; }
		public ICarrier Carrier { get; protected set; }

		public NewCarrierEventArgs(IReceptorInstance from, ICarrier carrier)
		{
			From = from;
			Carrier = carrier;
		}
	}

	/// <summary>
	/// Internal class for managing the queued carrier and the action to carry out when 
	/// a receptor receiving the carrier's protocol becomes available.
	/// </summary>
	internal class QueuedCarrierAction
	{
		public Carrier Carrier { get; set; }
		public Action Action { get; set; }
	}

	/// <summary>
	/// Container for all registered receptors, as well as providing managing the creation
	/// of carriers and the execution of a carrier's protocol on receiving receptors.
	/// </summary>
    public class ReceptorsContainer :  IReceptorSystem
    {
		/// <summary>
		/// Fires when a new receptor is instantiated and registered into the system.
		/// </summary>
		public event EventHandler<ReceptorEventArgs> NewReceptor;

		/// <summary>
		/// Fires when a new carrier is created.
		/// </summary>
		public event EventHandler<NewCarrierEventArgs> NewCarrier;

		/// <summary>
		/// Fires when a receptor is removed.
		/// </summary>
		public event EventHandler<ReceptorEventArgs> ReceptorRemoved;

		/// <summary>
		/// The collection of receptors currently in the system.
		/// </summary>
		public List<Receptor> Receptors { get; protected set; }

		/// <summary>
		/// The Semantic Type System instance that defines the carrier protocols.
		/// </summary>
		public ISemanticTypeSystem SemanticTypeSystem { get; protected set; }

		/// <summary>
		/// The list of receptors to which each protocol maps.
		/// </summary>
		protected Dictionary<string, List<Receptor>> protocolReceptorMap;

		/// <summary>
		/// A map of registered receptors.  These are receptors that are instantiated
		/// and whose protocols have been associated in the protocolReceptorMap.
		/// </summary>
		protected  Dictionary<Receptor, bool> registeredReceptorMap;

		/// <summary>
		/// Internal list of global receptors, which are handled slightly differently
		/// as they receive all carriers.
		/// </summary>
		protected List<Receptor> globalReceptors;

		/// <summary>
		/// The list of queued carriers because there are no receptors available to process the 
		/// carrier's protocol.
		/// </summary>
		private List<QueuedCarrierAction> queuedCarriers;

		/// <summary>
		/// Constructor, initializes internal collections.
		/// </summary>
		public ReceptorsContainer(ISemanticTypeSystem sts)
		{
			SemanticTypeSystem = sts;
			Initialize();
		}

		/// <summary>
		/// Adds a receptor definition to the collection.  Call this method when mass-loading
		/// receptors with their names and implementing assembly names, say from an XML file.
		/// </summary>
		public Receptor RegisterReceptor(string receptorName, string assemblyName)
		{
			Receptor r = new Receptor(receptorName, assemblyName);
			r.EnabledStateChanged += WhenEnabledStateChanged;
			Receptors.Add(r);

			return r;
		}

		/// <summary>
		/// Add an existing implementor of IReceptorInstance.  Call this method when registering
		/// application-level instances that are themselves receptors, such as models, controllers,
		/// views, etc.
		/// </summary>
		public void RegisterReceptor(string name, IReceptorInstance inst)
		{
			Receptor r = new Receptor(name, inst);
			r.EnabledStateChanged += WhenEnabledStateChanged;
			Receptors.Add(r);
		}

		/// <summary>
		/// Register a receptor given just the assembly filename.  The receptor assembly must still be loaded
		/// by calling LoadReceptors.  Used for registering receptors in a batch before instantiating them.
		/// </summary>
		/// <param name="filename"></param>
		public void RegisterReceptor(string filename)
		{
			Receptor r = Receptor.FromFile(filename).Instantiate(this);
			Receptors.Add(r);
		}

		/// <summary>
		/// When all current receptors have been registered (see Register... methods), call this method to
		/// instantiate (if necessary) and register the receptors.  This also generates the 
		/// protocol-receptor map for the currently registered receptors.
		/// </summary>
		public void LoadReceptors(Action<Receptor> afterRegister = null)
		{
			int processedCount = 0;
			string receptorName;

			// TODO: Refactor out of this code.
			Say("Loading receptors.");

			foreach (Receptor r in Receptors)
			{
				// TODO: This is lame, to prevent existing receptors from being re-processed.
				if (!r.Instantiated)
				{
					r.LoadAssembly().Instantiate(this);
				}

				// Get the carriers this receptor is interested in.
				if (!registeredReceptorMap.ContainsKey(r))
				{
					registeredReceptorMap[r] = true;

					// Get the protocols that this receptor is receiving, updating the protocolReceptorMap, adding this
					// receptor to the specified protocols.
					GatherProtocolReceivers(r);

					if (afterRegister != null)
					{
						afterRegister(r);
					}

					// Let interested parties know that we have a new receptor and handle how we want to announce the fact.
					// TODO: Refactor the announcement out of this code.
					NewReceptor.Fire(this, new ReceptorEventArgs(r));

					// Let the receptor instance perform additional initialization, such as creating carriers.
					r.Instance.Initialize();
					++processedCount;
					receptorName = r.Name;
				}
			}

			// Any queued carriers are now checked to determine whether receptors now exist to process their protocols.
			ProcessQueuedCarriers();

			// If we've loaded only one receptor...
			// TODO: Refactor this out of here.
			if (processedCount == 1)
			{
				Say("Receptor " + Receptors[0].Name + " online.");
			}
			else
			{
				Say("Receptors online.");
			}
		}

		/// <summary>
		/// Kludge to get protocols ending with "Recordset".
		/// </summary>
		public List<string> GetProtocolsEndingWith(string match)
		{
			List<string> ret = new List<string>();

			ret.AddRange(SemanticTypeSystem.SemanticTypes.Keys.Where(k => k.EndsWith(match)));

			return ret;
		}

		/// <summary>
		/// Create a carrier of the specified protocol and signal.
		/// </summary>
		/// <param name="from">The source receptor.  Cay be null.</param>
		/// <param name="protocol">The protocol.</param>
		/// <param name="signal">The signal in the protocol's format.</param>
		public ICarrier CreateCarrier(IReceptorInstance from, ISemanticTypeStruct protocol, dynamic signal)
		{
			// This calls the internal method with recursion set to false.  We don't want to expose this 
			// flag, so this method is a public front, as receptors should never set the "stop recursion" flag
			// to true when creating carriers.
			return CreateCarrier(from, protocol, signal, false);
		}

		/// <summary>
		/// Create a carrier of the specified protocol and signal if a receiver currently exists.
		/// Note that because a carrier might not be created, there is no return value for this method.
		/// </summary>
		/// <param name="from">The source receptor.  Cay be null.</param>
		/// <param name="protocol">The protocol.</param>
		/// <param name="signal">The signal in the protocol's format.</param>
		public void CreateCarrierIfReceiver(IReceptorInstance from, ISemanticTypeStruct protocol, dynamic signal)
		{
			if (protocolReceptorMap.ContainsKey(protocol.DeclTypeName))
			{
				CreateCarrier(from, protocol, signal);
			}
		}

		/// <summary>
		/// Create a carrier for internal purposes only, perhaps to pass as a sub-protocol in a parent carrier.
		/// </summary>
		public ICarrier CreateInternalCarrier(ISemanticTypeStruct protocol, dynamic signal)
		{
			return new Carrier(protocol, signal);
		}

		/// <summary>
		/// Removes a receptor from the system.  Before being removed, the receptor's Terminate() 
		/// method is called so that it can do whatever cleanup is required.
		/// </summary>
		/// <param name="receptor"></param>
		public void Remove(IReceptor receptor)
		{
			receptor.Instance.Terminate();
			// TODO: If our collections were IReceptor, then we wouldn't need the "as".
			Receptors.Remove(receptor as Receptor);
			protocolReceptorMap.ForEach(kvp => kvp.Value.Remove(receptor as Receptor));
			ReceptorRemoved.Fire(this, new ReceptorEventArgs(receptor));

			// TODO: Refactor out of this code.
			// TODO: Add a "RemovedReceptor" event.
			Say("Receptor " + receptor.Name + " removed.");
		}

		/// <summary>
		/// Remove the receptor container of the specified instance.
		/// </summary>
		/// <param name="receptorInstance"></param>
		public void Remove(IReceptorInstance receptorInstance)
		{
			// Clone the list because the master list will change.
			Receptors.Where(r => r.Instance == receptorInstance).ToList().ForEach(r => Remove(r));
		}

		/// <summary>
		/// Reset the receptor system.  This allows the current carriers to cleanly terminate
		/// and then re-initializes the internal collections to an empty state.
		/// </summary>
		public void Reset()
		{
			Receptors.ForEach(r => r.Instance.Terminate());
			Initialize();
		}

		/// <summary>
		/// Clears out all data.
		/// </summary>
		protected void Initialize()
		{
			Receptors = new List<Receptor>();
			protocolReceptorMap = new Dictionary<string, List<Receptor>>();
			queuedCarriers = new List<QueuedCarrierAction>();
			globalReceptors = new List<Receptor>();
			registeredReceptorMap = new Dictionary<Receptor, bool>();
		}

		/// <summary>
		/// Get the protocols that this receptor is receiving, updating the protocolReceptorMap, adding this
		/// receptor to the specified protocols.
		/// </summary>
		protected void GatherProtocolReceivers(Receptor r)
		{
			// For each protocol...
			r.Instance.GetReceiveProtocols().ForEach(protocolName =>
				{
					// If it's a wildcard receptor, we handle it differently because "*" is a magic protocol.
					if (protocolName == "*")
					{
						// This is a global receiver.  Attach it to all current carrier receptors, but don't create an instance in the CarrierReceptorMap.
						protocolReceptorMap.ForEach(kvp => kvp.Value.Add(r));
						globalReceptors.Add(r);
					}
					else
					{
						// Get the list of receiving receptors for the protocol, or, if it doesn't exist, create it.
						List<Receptor> receivingReceptors;

						if (!protocolReceptorMap.TryGetValue(protocolName, out receivingReceptors))
						{
							receivingReceptors = new List<Receptor>();
							protocolReceptorMap[protocolName] = receivingReceptors;
							// Append all current global receptors to this protocol - receptor map.
							globalReceptors.ForEach(gr => receivingReceptors.Add(gr));
						}

						// Associate the receptor with the protocol it receives.
						protocolReceptorMap[protocolName].Add(r);
					}
				});
		}

		/// <summary>
		/// Internal carrier creation.  This includes the "stopRecursion" flag to prevent wildcard receptors from receiving ad-infinitum their own emissions.
		/// </summary>
		protected ICarrier CreateCarrier(IReceptorInstance from, ISemanticTypeStruct protocol, dynamic signal, bool stopRecursion)
		{
			Carrier carrier = new Carrier(protocol, signal);
			NewCarrier.Fire(this, new NewCarrierEventArgs(from, carrier));

			// We pass along the stopRecursion flag to prevent wild-card carrier receptor from receiving their own emissions, which would result in a new carrier,
			// ad-infinitum.
			ProcessReceptors(from, carrier, stopRecursion);

			return carrier;
		}

		/// <summary>
		/// Return true if receptors exist and are enabled for this protocol.
		/// </summary>
		protected bool HaveEnabledReceptors(ISemanticTypeStruct protocol)
		{
			bool found = false;
			List<Receptor> receptors; 
			bool haveCarrierMap = protocolReceptorMap.TryGetValue(protocol.DeclTypeName, out receptors);

			if (haveCarrierMap)
			{
				found = receptors.Exists(r => r.Enabled);
			}

			return found;
		}

		/// <summary>
		/// Given a carrier, if there are receptors for the carrier's protocol, act upon the carrier immediately.
		/// If there are no receptors for the protocol, queue the carrier.
		/// </summary>
		protected void ProcessReceptors(IReceptorInstance from, Carrier carrier, bool stopRecursion)
		{
			// Get the action that we are supposed to perform on the carrier.
			Action action = GetProcessAction(from, carrier, stopRecursion);
			List<Receptor> receptors;

			bool haveCarrierMap = protocolReceptorMap.TryGetValue(carrier.Protocol.DeclTypeName, out receptors);

			// If we have any enabled receptor for this carrier (a mapping of carrier to receptor list exists and receptors actually exist in that map)...
			if (haveCarrierMap && receptors.Where(r => r.Enabled).Count() > 0)
			{
				// ...perform the action.
				action();
			}
			else
			{
				// ...othwerise, queue up the carrier for when there is a receptor for it.
				queuedCarriers.Add(new QueuedCarrierAction() { Carrier = carrier, Action = action });
			}
		}

		/// <summary>
		/// If a queued carrier has a receptor to receive the protocol, execute the action and remove it from the queue.
		/// </summary>
		protected void ProcessQueuedCarriers()
		{
			List<QueuedCarrierAction> removeActions = new List<QueuedCarrierAction>();

			// An action can result in carriers being created and queued if there's no receptor, so we walk through the existing 
			// collection with an indexer rather than a foreach.
			queuedCarriers.IndexerForEach(action =>
			{
				List<Receptor> receptors;

				bool haveCarrierMap = protocolReceptorMap.TryGetValue(action.Carrier.Protocol.DeclTypeName, out receptors);

				// If we have any enabled receptor for this carrier (a mapping of carrier to receptor list exists and receptors actually exist in that map)...
				if (haveCarrierMap && receptors.Where(r => r.Enabled).Count() > 0)
				{
					action.Action();
					// Collect actions that need to be removed.
					removeActions.Add(action);
				}
			});

			// Remove all processed actions.
			removeActions.ForEach(a => queuedCarriers.Remove(a));
		}

		/// <summary>
		/// Return an action representing what to do for a new carrier/protocol.
		/// </summary>
		protected Action GetProcessAction(IReceptorInstance from, Carrier carrier, bool stopRecursion)
		{
			// Construct an action...
			Action action = new Action(() =>
				{
					// Get the receptors receiving the protocol.
					var receptors = protocolReceptorMap[carrier.Protocol.DeclTypeName];

					// For each receptor that is enabled...
					receptors.Where(r=>r.Enabled).ForEach(receptor =>
					{
						// The action is "ProcessCarrier".
						// TODO: *** Pass in the carrier, not the carrier's fields!!! ***
						Action process = new Action(() => receptor.Instance.ProcessCarrier(carrier));

						// TODO: This flag is tied in with the visualizer, we should extricate this flag and logic.
						if (receptor.Instance.IsHidden)
						{
							// Don't visualize carriers to hidden receptors.
							process();
						}
						else if (!stopRecursion)
						{
							// TODO: This should be handled externally somehow.
							// Defer the action, such that the visualizer can invoke it when it determines the carrier rendering to the receiving receptor has completed.
							ISemanticTypeStruct protocol = SemanticTypeSystem.GetSemanticTypeStruct("CarrierAnimation");
							dynamic signal = SemanticTypeSystem.Create("CarrierAnimation");
							signal.Process = process;
							signal.From = from;
							signal.To = receptor.Instance;
							signal.Carrier = carrier;
							CreateCarrier(null, protocol, signal, receptor.Instance.GetReceiveProtocols().Contains("*"));
						}
					});
				});

			return action;
		}

		/// <summary>
		/// TODO: Remove this to outside this class.
		/// </summary>
		/// <param name="r"></param>
		protected void AnnounceReceptor(IReceptor r)
		{
			Say("Receptor " + r.Name + " online.");
		}

		// TODO: Duplicate code.
		// TODO: We probably could use some global carrier creation library.
		protected void Say(string msg)
		{
			ISemanticTypeStruct protocol = SemanticTypeSystem.GetSemanticTypeStruct("TextToSpeech");
			dynamic signal = SemanticTypeSystem.Create("TextToSpeech");
			signal.Text = msg;
			CreateCarrier(null, protocol, signal);
		}

		protected void WhenEnabledStateChanged(object sender, ReceptorEnabledEventArgs e)
		{
			// Any queued carriers are now checked to determine whether receptors are now enabled to process their protocols.
			ProcessQueuedCarriers();
		}
	}
}
