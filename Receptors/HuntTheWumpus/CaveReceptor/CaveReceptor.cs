using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.ExtensionMethods;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace HuntTheWumpus
{
	public class CaveReceptor : BaseReceptor
    {
		public override string Name { get { return "Cave"; } }

		protected Guid id;
		protected int caveNumber;
		protected int[] caveNeighbors;

		protected bool hasBottomlessPit;
		protected bool hasSuperBats;
		protected bool hasWumpus;
		protected bool hasPlayer;

		protected bool HasItemOfInterest
		{
			get { return hasBottomlessPit || hasSuperBats || hasWumpus; }
		}

		public CaveReceptor(IReceptorSystem rsys)
			: base(rsys)
		{
			receiveProtocols.Add("HW_YouAre");
			emitProtocols.Add("HW_WhereAmI");
			emitProtocols.Add("Text");
			caveNeighbors = new int[3];
		}

		public override void Initialize()
		{
			base.Initialize();
			id = Guid.NewGuid();
			CreateCarrier("HW_WhereAmI", (signal) => signal.ID = id);
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			// Is it the "You are here" protocol?
			if (carrier.Protocol.DeclTypeName == "HW_YouAre")
			{
				// And is it meant for me? (poor man's filtering for now!)
				if (carrier.Signal.ID == id)
				{
					// Save our cave # and neighbor cave numbers.
					caveNumber = carrier.Signal.CaveNumber;
					caveNeighbors[0] = carrier.Signal.AdjoiningCave1;
					caveNeighbors[1] = carrier.Signal.AdjoiningCave2;
					caveNeighbors[2] = carrier.Signal.AdjoiningCave3;

					hasBottomlessPit = carrier.Signal.HasBottomlessPit;
					hasSuperBats = carrier.Signal.HasSuperBats;
					hasWumpus = carrier.Signal.HasWumpus;
					hasPlayer = carrier.Signal.HasWumpus;

					// Configure emitters and listeners.
					UpdateEmitters();
					UpdateListeners();

					if (hasPlayer)
					{
						AskAboutOurNeighbors();
						SayWhoIsNextToUs();
					}
				}
			}
			else if (carrier.Protocol.DeclTypeName.StartsWith("HW_RequestInfo"))
			{
				int fromCave = carrier.Signal.FromCaveNumber;

				if (HasItemOfInterest)
				{
					CreateCarrier("HW_ProcessInfo" + fromCave, (signal) =>
						{
							signal.HaveBottomlessPit = hasBottomlessPit;
							signal.HaveSuperBats = hasSuperBats;
							signal.HaveWumpus = hasWumpus;
						});
				}
			}
			else if (carrier.Protocol.DeclTypeName.StartsWith("HW_ProcessInfo"))
			{
				dynamic signal = carrier.Signal;

				if (signal.HaveBottomlessPit)
				{
					CreateCarrier("Text", (outSignal) => outSignal.Value = "I feel a draft!");
				}

				if (signal.HaveSuperBats)
				{
					CreateCarrier("Text", (outSignal) => outSignal.Value = "I hear flapping!");
				}

				if (signal.HaveWumpus)
				{
					CreateCarrier("Text", (outSignal) => outSignal.Value = "I smell a Wumpus!");
				}
			}
		}

		protected void UpdateEmitters()
		{
			// I will make requests to my neighbors for info with specific protocols.
			caveNeighbors.ForEach(cn =>
				{
					// Request info from our neighbors.
					AddEmitProtocol("HW_RequestInfo" + cn);

					// Send info back to my neighbors.
					AddEmitProtocol("HW_ProcessInfo" + cn);
				});

		}

		protected void UpdateListeners()
		{
			// Receive requests for my info.
			AddReceiveProtocol("HW_RequestInfo" + caveNumber);

			// Receive the info to process from my neighbor.
			AddReceiveProtocol("HW_ProcessInfo" + caveNumber);
		}

		protected void AskAboutOurNeighbors()
		{
			caveNeighbors.ForEach(cn => CreateCarrier("HW_RequestInfo" + cn, (signal) => { signal.FromCaveNumber = caveNumber; }));
		}

		protected void SayWhoIsNextToUs()
		{
			CreateCarrier("Text", (outSignal) => outSignal.Value = "You are in cave number "+caveNumber);
			CreateCarrier("Text", (outSignal) => outSignal.Value = "Passages lead to " + String.Join(", ", caveNeighbors.Select(cn => cn.ToString())));
		}
	}
}
