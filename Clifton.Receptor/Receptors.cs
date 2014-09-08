using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	/// Internal class for managing the queued carrier and the action to carry out when 
	/// a receptor receiving the carrier's protocol becomes available.
	/// </summary>
	internal class QueuedCarrierAction
	{
		public IReceptor From { get; set; }
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
		// public List<Receptor> Receptors { get; protected set; }
		// Yuck.  Way to much conversion going on here.
		public ReadOnlyCollection<IReceptor> Receptors { get { return receptors.Cast<IReceptor>().ToList().AsReadOnly(); } }

		/// <summary>
		/// The Semantic Type System instance that defines the carrier protocols.
		/// </summary>
		public ISemanticTypeSystem SemanticTypeSystem { get; protected set; }

		/// <summary>
		/// The list of receptors to which each protocol maps.
		/// This list supports unmapped "from receptors" in the registered receptor map,
		/// allowing the "system" to drop carriers into a particular membrane and have that
		/// carrier be processed by a receive-only receptor in the membrane.
		/// </summary>
		protected Dictionary<string, List<IReceptor>> protocolReceptorMap;

		/// <summary>
		/// A map of registered receptors.  These are receptors that are instantiated
		/// and whose protocols have been associated in the protocolReceptorMap.
		/// </summary>
		protected  Dictionary<IReceptor, bool> registeredReceptorMap;

		/// <summary>
		/// Internal list of global receptors, which are handled slightly differently
		/// as they receive all carriers.
		/// </summary>
		protected List<IReceptor> globalReceptors;

		/// <summary>
		/// The list of queued carriers because there are no receptors available to process the 
		/// carrier's protocol.
		/// </summary>
		private List<QueuedCarrierAction> queuedCarriers;

		/// <summary>
		/// Internal collection of receptors in this receptor system.
		/// </summary>
		protected List<Receptor> receptors;

		// TODO: The receptors container should acquire this list, rather than it being set.
		/// <summary>
		/// The master list of receptor connections, which oddly at this point is being computed
		/// by the visualizer.
		/// </summary>
		public Dictionary<IReceptor, List<IReceptor>> MasterReceptorConnectionList { get; set; }

		public IReceptor this[string name] { get { return receptors.Single(r => r.Name == name); } }

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
			receptors.Add(r);

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
			receptors.Add(r);
		}

		/// <summary>
		/// Register a receptor given just the assembly filename.  The receptor assembly must still be loaded
		/// by calling LoadReceptors.  Used for registering receptors in a batch before instantiating them.
		/// </summary>
		/// <param name="filename"></param>
		public IReceptor RegisterReceptor(string filename)
		{
			Receptor r = Receptor.FromFile(filename).Instantiate(this);
			receptors.Add(r);

			return r;
		}

		/// <summary>
		/// When all current receptors have been registered (see Register... methods), call this method to
		/// instantiate (if necessary) and register the receptors.  This also generates the 
		/// protocol-receptor map for the currently registered receptors.
		/// </summary>
		public void LoadReceptors(Action<IReceptor> afterRegister = null)
		{
			int processedCount = 0;
			string receptorName;
			List<Receptor> newReceptors = new List<Receptor>();

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
					newReceptors.Add(r);
					++processedCount;
					receptorName = r.Name;
				}
			}

			// This order is important.  The visualizer needs to know all the receptors within this membrane AFTER
			// the receptors have been instantiated.  Secondly, the receptors can't be initialized until the visualizer
			// knows where they are.

			// Let interested parties know that we have new receptors and handle how we want to announce the fact.
			newReceptors.ForEach(r =>
			{
				if (afterRegister != null)
				{
					afterRegister(r);
				}

				NewReceptor.Fire(this, new ReceptorEventArgs(r));
			});

			// Let the receptor instance perform additional initialization, such as creating carriers.
			// We do this only for enabled receptors.
			newReceptors.Where(r=>r.Enabled).ForEach(r => r.Instance.Initialize());

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
			ICarrier carrier = CreateCarrier(from, protocol, signal, false);

			return carrier;
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
			if (TargetReceptorExistsFor(ReceptorFromInstance(from), protocol))
			{
				CreateCarrier(from, protocol, signal);
			}
			else
			{
				// Recurse into SE's of the protocol and emit carriers for those as well, if a receiver exists.
				// We do this even if there isn't a target for the top-level receptor.
				CreateCarriersForSemanticElements(from, protocol, signal, false);
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
			receptors.Remove(receptor as Receptor);
			registeredReceptorMap.Remove(receptor);
			protocolReceptorMap.ForEach(kvp => kvp.Value.Remove(receptor as Receptor));
			ReceptorRemoved.Fire(this, new ReceptorEventArgs(receptor));

			// TODO: Refactor out of this code.
			// TODO: Add a "RemovedReceptor" event.
			Say("Receptor " + receptor.Name + " removed.");
		}

		/// <summary>
		/// Call on each receptor instance when the system has been fully initialized (applet or a receptor is dropped onto the surface.)
		/// </summary>
		public void EndSystemInit()
		{
			receptors.ForEach(r => r.Instance.EndSystemInit());
		}

		/// <summary>
		/// Remove the receptor container of the specified instance.
		/// </summary>
		/// <param name="receptorInstance"></param>
		public void Remove(IReceptorInstance receptorInstance)
		{
			// Clone the list because the master list will change.
			Remove((Receptor)ReceptorFromInstance(receptorInstance));
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

		// TODO: Re-implement by providing methods to add/remove instantiated receptors which
		// would support moving receptors between membranes.  This is a brute force approach
		// for now.
		/// <summary>
		/// Recreates the protocol receptor map from scratch.
		/// </summary>
		public void ReloadProtocolReceptorMap()
		{
			protocolReceptorMap.Clear();
			Receptors.ForEach(r => GatherProtocolReceivers(r));
		}

		public void MoveReceptorTo(IReceptor receptor, ReceptorsContainer target)
		{
			InternalRemove(receptor);
			target.InternalAdd(receptor);
		}

		/// <summary>
		/// Remove the receptor without generating remove events or terminating the receptor.
		/// </summary>
		protected void InternalRemove(IReceptor receptor)
		{
			receptors.Remove(receptor as Receptor);
			registeredReceptorMap.Remove(receptor);
			protocolReceptorMap.ForEach(kvp => kvp.Value.Remove(receptor as Receptor));
		}

		/// <summary>
		/// Add the receptor, without generating add events.
		/// </summary>
		protected void InternalAdd(IReceptor receptor)
		{
			// TODO: If receptors was List<IReceptor>, we wouldn't need this cast.
			receptors.Add(receptor as Receptor);
			registeredReceptorMap[receptor] = true;
			// The receptor instance is now using this receptor system!
			receptor.Instance.ReceptorSystem = this;
			// Process any queued carriers that may now become active.
			ReloadProtocolReceptorMap();
			ProcessQueuedCarriers();
		}

		/// <summary>
		/// Clears out all data.
		/// </summary>
		protected void Initialize()
		{
			receptors = new List<Receptor>();
			protocolReceptorMap = new Dictionary<string, List<IReceptor>>();
			queuedCarriers = new List<QueuedCarrierAction>();
			globalReceptors = new List<IReceptor>();
			registeredReceptorMap = new Dictionary<IReceptor, bool>();
			MasterReceptorConnectionList = new Dictionary<IReceptor, List<IReceptor>>();
		}

		/// <summary>
		/// Get the protocols that this receptor is receiving, updating the protocolReceptorMap, adding this
		/// receptor to the specified protocols.
		/// </summary>
		protected void GatherProtocolReceivers(IReceptor r)
		{
			// For each protocol...
			r.Instance.GetReceiveProtocols().ForEach(rq =>
				{
					// If it's a wildcard receptor, we handle it differently because "*" is a magic protocol.
					if (rq.Protocol == "*")
					{
						// This is a global receiver.  Attach it to all current carrier receptors, but don't create an instance in the CarrierReceptorMap.
						protocolReceptorMap.ForEach(kvp => kvp.Value.Add(r));
						globalReceptors.Add(r);
					}
					else
					{
						// Get the list of receiving receptors for the protocol, or, if it doesn't exist, create it.
						List<IReceptor> receivingReceptors;

						if (!protocolReceptorMap.TryGetValue(rq.Protocol, out receivingReceptors))
						{
							receivingReceptors = new List<IReceptor>();
							protocolReceptorMap[rq.Protocol] = receivingReceptors;
							// Append all current global receptors to this protocol - receptor map.
							globalReceptors.ForEach(gr => receivingReceptors.Add(gr));
						}

						// Associate the receptor with the protocol it receives.
						protocolReceptorMap[rq.Protocol].Add(r);
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

			// Recurse into SE's of the protocol and emit carriers for those as well, if a receiver exists.
			CreateCarriersForSemanticElements(from, protocol, signal, stopRecursion);

			return carrier;
		}

		/// <summary>
		/// Recurse into SE's of the protocol and emit carriers for those as well, if a receiver exists.
		/// </summary>
		protected void CreateCarriersForSemanticElements(IReceptorInstance from, ISemanticTypeStruct protocol, dynamic signal, bool stopRecursion)
		{
			protocol.SemanticElements.ForEach(se =>
				{
					dynamic subsignal = SemanticTypeSystem.Clone(signal, se); // Clone the contents of the signal's semantic element into the subsignal.
					ISemanticTypeStruct semStruct = SemanticTypeSystem.GetSemanticTypeStruct(se.Name);
					// Will result in recursive calls for all sub-semantic types.
					CreateCarrierIfReceiver(from, semStruct, subsignal);
				});
		}

		/// <summary>
		/// Returns true if there is an enabled target from the specified receptor with the specified protocol.
		/// </summary>
		protected bool TargetReceptorExistsFor(IReceptor from, ISemanticTypeStruct protocol)
		{
			bool ret = false;

			List<IReceptor> targets = new List<IReceptor>();
			if (MasterReceptorConnectionList.TryGetValue(from, out targets))
			{
				// We're only interested in enabled receptors.
				ret = targets.Any(r => r != from && r.Enabled && r.Instance.GetReceiveProtocols().Select(rp=>rp.Protocol).Contains(protocol.DeclTypeName));
			}

			if (!ret)
			{
				// check protocol map for receivers that are not the issuing receptor:
				ret = protocolReceptorMap.Any(kvp => (kvp.Key == protocol.DeclTypeName) && kvp.Value.Any(r => (r != from) && (r.Enabled))); // .ContainsKey(protocol.DeclTypeName);
			}

			return ret;
		}

		// TODO: This code needs to be optimized.
		/// <summary>
		/// Returns the target receptors that will receive the carrier protocol, qualified by the receptor's optional condition on the signal.
		/// </summary>
		protected List<IReceptor> GetTargetReceptorsFor(IReceptor from, ICarrier carrier)
		{
			List<IReceptor> targets;
			ISemanticTypeStruct protocol = carrier.Protocol;

			// When the try fails, it sets targets to null.
			if (!MasterReceptorConnectionList.TryGetValue(from, out targets))
			{
				targets = new List<IReceptor>();
			}

			// Only enabled receptors and receptors that are not the source of the carrier.
			List<IReceptor> filteredTargets = targets.Where(r => r != from && r.Enabled && r.Instance.GetReceiveProtocols().Select(rq => rq.Protocol).Contains(protocol.DeclTypeName)).ToList();

			// Will have a count of 0 if the receptor is the system receptor, ie, carrier animations or other protocols.
			// TODO: This seems kludgy, is there a better way of working with this?
			// Also, if the emitting receptor doesn't declare its protocol, this count will be 0, leading to potentially strange results.
			// For example, comment out the persistence receptors "IDReturn" and run the feed reader example.  You'll see that TWO items
			// are returned as matching "RSSFeed" table name and for reasons unknown at the moment, protocolReceptorMap has two entries that qualify.
			if (filteredTargets.Count == 0)
			{
				// When the try fails, it sets targets to null.
				if (protocolReceptorMap.TryGetValue(protocol.DeclTypeName, out targets))
				{
					filteredTargets = targets.Where(r => r.Enabled && (r != from)).ToList();
				}
			}

			// Lastly, filter the list by qualified receptors that are not the source of the carrier.
			List<IReceptor> newTargets = new List<IReceptor>();

			filteredTargets.Where(r=>r != from && r.Enabled).ForEach(t =>
				{
					// Get the list of receive actions and filters for the specific protocol.
					var receiveList = t.Instance.GetReceiveProtocols().Where(rp => rp.Protocol == protocol.DeclTypeName);
					receiveList.ForEach(r =>
						{
							// If qualified, add to the final target list.
							if (r.Qualifier(carrier.Signal))
							{
								newTargets.Add(t);
							}
						});
				});

			// filteredTargets = filteredTargets.Where(t => t.Instance.GetReceiveProtocols().Any(rp => (rp.Protocol == protocol.DeclTypeName) && rp.Qualifier(carrier.Signal))).ToList();

			// Get the targets of the single receive protocol that matches the DeclTypeName and whose qualifier returns true.
			// filteredTargets = filteredTargets.Where(t => t.Instance.GetReceiveProtocols().Single(rp => rp.Protocol == protocol.DeclTypeName).Qualifier(carrier.Signal)).ToList();

			return newTargets;
		}

		/// <summary>
		/// Given a carrier, if there are receptors for the carrier's protocol, act upon the carrier immediately.
		/// If there are no receptors for the protocol, queue the carrier.
		/// </summary>
		protected void ProcessReceptors(IReceptorInstance from, Carrier carrier, bool stopRecursion)
		{
			// Get the action that we are supposed to perform on the carrier.
			Action action = GetProcessAction(from, carrier, stopRecursion);
			List<IReceptor> receptors = GetTargetReceptorsFor(ReceptorFromInstance(from), carrier);

			// If we have any enabled receptor for this carrier (a mapping of carrier to receptor list exists and receptors actually exist in that map)...
			if (receptors.Count > 0)
			{
				// ...perform the action.
				action();
			}
			else
			{
				// ...othwerise, queue up the carrier for when there is a receptor for it.
				queuedCarriers.Add(new QueuedCarrierAction() { From = ReceptorFromInstance(from), Carrier = carrier, Action = action });
			}
		}

		/// <summary>
		/// If a queued carrier has a receptor to receive the protocol, execute the action and remove it from the queue.
		/// </summary>
		public void ProcessQueuedCarriers()
		{
			List<QueuedCarrierAction> removeActions = new List<QueuedCarrierAction>();

			// An action can result in carriers being created and queued if there's no receptor, so we walk through the existing 
			// collection with an indexer rather than a foreach.
			queuedCarriers.IndexerForEach(action =>
			{
				List<IReceptor> receptors = GetTargetReceptorsFor(action.From, action.Carrier);

				// If we have any enabled receptor for this carrier (a mapping of carrier to receptor list exists and receptors actually exist in that map)...
				if (receptors.Count > 0)
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
					List<IReceptor> receptors = GetTargetReceptorsFor(ReceptorFromInstance(from), carrier);

					// For each receptor that is enabled...
					receptors.ForEach(receptor =>
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
							// Simulate coming from the system, as it IS a system message.
							CreateCarrier(from, protocol, signal, receptor.Instance.GetReceiveProtocols().Select(rp=>rp.Protocol).Contains("*"));
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
			// TODO: I've turned this off because every membrane's receptor system is now talking!!!!

			//ISemanticTypeStruct protocol = SemanticTypeSystem.GetSemanticTypeStruct("TextToSpeech");
			//dynamic signal = SemanticTypeSystem.Create("TextToSpeech");
			//signal.Text = msg;
			//CreateCarrier(null, protocol, signal);
		}

		protected void WhenEnabledStateChanged(object sender, ReceptorEnabledEventArgs e)
		{
			// Any queued carriers are now checked to determine whether receptors are now enabled to process their protocols.
			ProcessQueuedCarriers();
		}

		protected IReceptor ReceptorFromInstance(IReceptorInstance inst)
		{
			return receptors.SingleOrDefault(r => r.Instance == inst);
		}
	}
}
