using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.ExtensionMethods;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.Receptor
{
	public class CarrierAction
	{
		public Carrier Carrier { get; set; }
		public Action Action { get; set; }
	}

	public class NewReceptorEventArgs : EventArgs
	{
		public IReceptor Receptor { get; protected set; }

		public NewReceptorEventArgs(IReceptor receptor)
		{
			Receptor = receptor;
		}
	}

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

    public class ReceptorsContainer :  IReceptorSystem
    {
		public event EventHandler<NewReceptorEventArgs> NewReceptor;
		public event EventHandler<NewCarrierEventArgs> NewCarrier;

		public List<Receptor> Receptors { get; protected set; }
		public List<Carrier> Carriers { get; protected set; }
		public ISemanticTypeSystem SemanticTypeSystem { get; set; }
		public Dictionary<string, List<Receptor>> CarrierReceptorMap { get; protected set; }

		protected List<Receptor> globalReceptors;
		protected List<CarrierAction> queuedCarriers;

		public ReceptorsContainer()
		{
			Initialize();
		}

		public void Reset()
		{
			Receptors.ForEach(r => r.Instance.Terminate());
			Initialize();
		}

		public void Remove(IReceptor receptor)
		{
			receptor.Instance.Terminate();
			// TODO: If our collections were IReceptor, then we wouldn't need the "as".
			Receptors.Remove(receptor as Receptor);
			CarrierReceptorMap.ForEach(kvp => kvp.Value.Remove(receptor as Receptor));
			// Remove all maps where their are no further receptors.
			// This allows us to accrue carriers when receptors are removed, as we no longer have a receptor receiver.
			// CarrierReceptorMap.ToList().Where(kvp => kvp.Value.Count == 0).ForEach(kvp => CarrierReceptorMap.Remove(kvp.Key));
			Say("Receptor " + receptor.Name + " removed.");
		}

		public void AddReceptorDefinition(string receptorName, string assemblyName)
		{
			Receptor r = new Receptor(receptorName, assemblyName);
			Receptors.Add(r);
		}

		public void AddReceptor(string name, IReceptorInstance inst)
		{
			Receptor r = new Receptor(name, inst);
			Receptors.Add(r);
		}

		public void LoadReceptors()
		{
			int processedCount = 0;
			string receptorName;

			if (Receptors.Any(r=>r.Processed==false))
			{
				Say("Loading receptors.");

				foreach (Receptor r in Receptors)
				{
					// TODO: This is lame, to prevent existing receptors from being re-processed.
					if (!r.Processed)
					{
						// If the instance hasn't been created yet...
						if (r.Instance == null)
						{
							r.LoadAssembly();
							r.Instantiate(this);
						}

						GatherCarrierReceivers(r);
						NewReceptor.Fire(this, new NewReceptorEventArgs(r));
						r.Processed = true;
						++processedCount;
						receptorName = r.Name;
					}
				}

				ProcessQueuedCarriers();

				// If we've loaded only one receptor...
				if (processedCount == 1)
				{
					Say("Receptor " + Receptors[0].Name + " online.");
				}
				else
				{
					Say("Receptors online.");
				}
			}
		}

		public void InstantiateReceptor(string filename)
		{
			Receptor r = new Receptor();
			r.FromFile(filename);
			r.Instantiate(this);
			r.Processed = true;
			Receptors.Add(r);
			GatherCarrierReceivers(r);
			NewReceptor.Fire(this, new NewReceptorEventArgs(r));
			AnnounceReceptor(r);
			ProcessQueuedCarriers();
		}

		public void CreateCarrier(IReceptorInstance from, ISemanticTypeStruct protocol, dynamic signal, bool stopRecursion = false)
		{
			Carrier carrier = new Carrier(protocol, signal);
			NewCarrier.Fire(this, new NewCarrierEventArgs(from, carrier));

			// We stop recursion when we get to a wild-card carrier receptor, otherwise, the "Carrier Animation" message recursively creates additional messages to the wild-card receptor.
			ProcessReceptors(from, carrier, stopRecursion);
		}

		protected void Initialize()
		{
			Receptors = new List<Receptor>();
			Carriers = new List<Carrier>();
			CarrierReceptorMap = new Dictionary<string, List<Receptor>>();
			queuedCarriers = new List<CarrierAction>();
			globalReceptors = new List<Receptor>();
		}

		protected void GatherCarrierReceivers(Receptor r)
		{
			r.Instance.GetReceiveCarriers().ForEach(n =>
				{
					if (n == "*")
					{
						// This is a global receiver.  Attach it to all current carrier receptors, but don't create an instance in the CarrierReceptorMap.
						CarrierReceptorMap.ForEach(kvp => kvp.Value.Add(r));
						globalReceptors.Add(r);
					}
					else
					{
						List<Receptor> receivingReceptors;

						if (!CarrierReceptorMap.TryGetValue(n, out receivingReceptors))
						{
							receivingReceptors = new List<Receptor>();
							CarrierReceptorMap[n] = receivingReceptors;
							// Append all current global receptors to this carrier - receptor map.
							globalReceptors.ForEach(gr => receivingReceptors.Add(gr));
						}

						CarrierReceptorMap[n].Add(r);
					}
				});
		}

		protected void ProcessReceptors(IReceptorInstance from, Carrier carrier, bool stopRecursion)
		{
			Action action = GetProcessAction(from, carrier, stopRecursion);
			List<Receptor> receptors;

			bool haveCarrierMap = CarrierReceptorMap.TryGetValue(carrier.Protocol.DeclTypeName, out receptors);

			// If we have receptors for this carrier (a mapping of carrier to receptor list exists and receptors actually exist)...
			if (haveCarrierMap && receptors.Count > 0)
			{
				// Perform the action.
				action();
			}
			else
			{
				// Othwerise, queue up the carrier for when there is a receptor for it.
				queuedCarriers.Add(new CarrierAction() { Carrier = carrier, Action = action });
			}
		}

		protected void ProcessQueuedCarriers()
		{
			List<CarrierAction> removeActions = new List<CarrierAction>();

			foreach (CarrierAction action in queuedCarriers)
			{
				if (CarrierReceptorMap.ContainsKey(action.Carrier.Protocol.DeclTypeName))
				{
					action.Action();
					removeActions.Add(action);
				}
			}

			removeActions.ForEach(a => queuedCarriers.Remove(a));
		}

		protected Action GetProcessAction(IReceptorInstance from, Carrier carrier, bool stopRecursion)
		{
			Action action = new Action(() =>
				{
					var receptors = CarrierReceptorMap[carrier.Protocol.DeclTypeName];
					int numReceptors = receptors.Count;

					receptors.ForEach(receptor =>
					{
						Action process = new Action(() => receptor.Instance.ProcessCarrier(carrier.Protocol, carrier.Signal));

						if (receptor.Instance.IsHidden)
						{
							// Don't visualize carriers to hidden receptors.
							process();
						}
						else if (!stopRecursion)
						{
							ISemanticTypeStruct protocol = SemanticTypeSystem.GetSemanticTypeStruct("CarrierAnimation");
							dynamic signal = SemanticTypeSystem.Create("CarrierAnimation");
							signal.Process = process;
							signal.From = from;
							signal.To = receptor.Instance;
							signal.Carrier = carrier;
							CreateCarrier(null, protocol, signal, receptor.Instance.GetReceiveCarriers().Contains("*"));
						}
					});
				});

			return action;
		}

		protected void AnnounceReceptor(IReceptor r)
		{
			Say("Receptor " + r.Name + " online.");
		}

		// TODO: Duplicate code.
		protected void Say(string msg)
		{
			ISemanticTypeStruct protocol = SemanticTypeSystem.GetSemanticTypeStruct("TextToSpeech");
			dynamic signal = SemanticTypeSystem.Create("TextToSpeech");
			signal.Text = msg;
			CreateCarrier(null, protocol, signal);
		}
	
		//protected void ProcessCarrierImmediate(ISemanticTypeStruct protocol, dynamic signal)
		//{
		//	CarrierReceptorMap.SingleOrDefault(t => t.Key == protocol.DeclTypeName).Value.ForEach(receptor => receptor.Instance.ProcessCarrier(protocol, signal));
		//}
	}
}
