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
		protected Random rnd;

		protected bool HasItemOfInterest
		{
			get { return hasBottomlessPit || hasSuperBats || hasWumpus; }
		}

		public CaveReceptor(IReceptorSystem rsys)
			: base(rsys)
		{
			receiveProtocols.Add("HW_YouAre");
			receiveProtocols.Add("HW_MoveTo");
			receiveProtocols.Add("HW_ShootInto");
			emitProtocols.Add("HW_WhereAmI");
			emitProtocols.Add("HW_Player");
			emitProtocols.Add("Text");
			caveNeighbors = new int[3];
			rnd = new Random(20);			// use a specific seed for testing.
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
					hasPlayer = carrier.Signal.HasPlayer;

					// Configure emitters and listeners.
					UpdateEmitters();
					UpdateListeners();

					if (hasPlayer)
					{
						SayWhoIsNextToUs();
						AskAboutOurNeighbors();
						TalkToPlayer();
					}
				}
			}
			else if (carrier.Protocol.DeclTypeName.StartsWith("HW_Announce"))
			{
				if (hasBottomlessPit)
				{
					CreateCarrier("Text", (outSignal) => outSignal.Value = "I feel a draft!");
				}

				if (hasSuperBats)
				{
					CreateCarrier("Text", (outSignal) => outSignal.Value = "I hear flapping!");							
				}

				if (hasWumpus)
				{
					CreateCarrier("Text", (outSignal) => outSignal.Value = "I smell a Wumpus!");
				}
			}
			else if (carrier.Protocol.DeclTypeName == "HW_MoveTo")
			{
				if (carrier.Signal.NewCaveNumber == caveNumber)
				{
					hasPlayer = true;
					
					if (CheckCaveState())
					{
						SayWhoIsNextToUs();
						AskAboutOurNeighbors();
						TalkToPlayer();
					}
				}
				else
				{
					hasPlayer = false;
				}
			}
			else if (carrier.Protocol.DeclTypeName == "HW_ShootInto")
			{
				if (carrier.Signal.CaveNumber == caveNumber)
				{
					if (hasPlayer)
					{
						CreateCarrier("Text", (outSignal) => outSignal.Value = "Ouch!  You shot yourself!!!!!!!!");
						CreateCarrier("HW_GameState", (outSignal) => outSignal.PlayerShotSelf = true);
					}
					// This is my cave the hunter is shooting into!
					else if (hasWumpus)
					{
						CreateCarrier("Text", (outSignal) => outSignal.Value = "Ouch!  You shot the Wumpus!!!!!!!!");
						CreateCarrier("HW_GameState", (outSignal) => outSignal.WumpusIsDead = true);
					}
					else
					{
						int arrowLife = carrier.Signal.RemainingLife;
						--arrowLife;

						if (arrowLife > 0)
						{
							// The arrow continues to a random room.
							CreateCarrier("HW_ShootInto", (signal) =>
							{
								signal.CaveNumber = caveNeighbors[rnd.Next(3)];
								signal.RemainingLife = arrowLife;
							});
						}
					}
				}
			}
		}

		protected void UpdateEmitters()
		{
			// I will make ask my neighbors to announce interesting things about themselves.
			caveNeighbors.ForEach(cn =>
				{
					AddEmitProtocol("HW_Announce" + cn);
				});
		}

		protected void UpdateListeners()
		{
			// I will respond to an announcement request.
			AddReceiveProtocol("HW_Announce" + caveNumber);
		}

		protected void AskAboutOurNeighbors()
		{
			caveNeighbors.ForEach(cn => CreateCarrier("HW_Announce" + cn, (signal) => { }));
		}

		protected void SayWhoIsNextToUs()
		{
			CreateCarrier("Text", (outSignal) => outSignal.Value = "You are in cave number "+caveNumber);
			CreateCarrier("Text", (outSignal) => outSignal.Value = "Passages lead to " + String.Join(", ", caveNeighbors.Select(cn => cn.ToString())));
		}

		protected void TalkToPlayer()
		{
			CreateCarrier("HW_Player", (outSignal) =>
				{
					outSignal.CaveNumber = caveNumber;
					outSignal.AdjoiningCave1 = caveNeighbors[0];
					outSignal.AdjoiningCave2 = caveNeighbors[1];
					outSignal.AdjoiningCave3 = caveNeighbors[2];
				});
		}

		protected bool CheckCaveState()
		{
			bool ret = !(hasSuperBats || hasWumpus || hasBottomlessPit);

			// Nothing save you from the Wumpus.  Test first.
			if (hasWumpus)
			{
				// In the original game, I believe bumping into a wumpus woke him and he either ate you
				// or moved to another room.
				CreateCarrier("Text", (outSignal) => outSignal.Value = "gnom, gnom, crunch!  You've been eaten by the Wumpus!!!!!!!");
				CreateCarrier("HW_GameState", (outSignal) => outSignal.PlayerEatenByWumpus = true);
			}
			else if (hasSuperBats)
			{
				// Bats will save you from a bottomless pit.
				CreateCarrier("Text", (outSignal) => outSignal.Value = "Super-Bat Snatch!!!!!!!");
				CreateCarrier("HW_GameState", (outSignal) => outSignal.SuperBatSnatch = true);
			}
			else if (hasBottomlessPit)
			{
				CreateCarrier("Text", (outSignal) => outSignal.Value = "AAAAAYYYYyyyyyeeeeeee  You fell into a bottomless pit!!!!!!!");
				CreateCarrier("HW_GameState", (outSignal) => outSignal.PlayerFellIntoPit = true);
			}

			return ret;
		}
	}
}
