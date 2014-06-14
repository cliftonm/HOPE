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
	public class CaveConfigurationReceptor : BaseReceptor
	{
		public override string Name { get { return "Cave Configuration"; } }

		protected int[][] caveMatrix = new int[][] 
		{
			new int[] {1, 5, 4}, new int[] { 0,  7,  2}, new int[] { 1,  9,  3}, new int[] { 2, 11,  4}, 
			new int[] { 3, 13,  0}, new int[] { 0, 14,  6}, new int[] { 5, 16,  7}, new int[] { 1,  6,  8}, 
			new int[] { 7,  9, 17}, new int[]  { 2,  8, 10}, new int[]  { 9, 11, 18}, new int[]  {10,  3, 12}, 
			new int[] {19, 11, 13}, new int[]  {14, 12,  4}, new int[]  {13,  5, 15}, new int[]  {14, 19, 16}, 
			new int[] { 6, 15, 17}, new int[] {16,  8, 18}, new int[] {10, 17, 19}, new int[] {12, 15, 18}
		};

		protected Dictionary<Guid, int> idToIndexMap;
		protected List<int> pits;
		protected List<int> bats;
		protected int wumpus;
		protected int player;
		protected Random rnd;

		// For simplicity in making assignments.
		protected int[] caveNumbers = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };

		public CaveConfigurationReceptor(IReceptorSystem rsys)
			: base(rsys)
		{
			idToIndexMap = new Dictionary<Guid, int>();
			pits = new List<int>();
			bats = new List<int>();
			receiveProtocols.Add("HW_WhereAmI");
			emitProtocols.Add("HW_YouAre");
			rnd = new Random(1);			// use a specific seed for testing.
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			dynamic signal = carrier.Signal;

			// Assign the ID to an index in the cave system that represent a vertex.
			idToIndexMap[signal.ID] = idToIndexMap.Count;

			// If we've got all 20 caves, tell each cave who it is and where it is.
			// The order in which the carriers are received here and by the cave receptors
			// is irrelevant.
			if (idToIndexMap.Count == 20)
			{
				AssignPits();
				AssignBats();
				AssignWumpus();
				AssignPlayer();

				idToIndexMap.ForEach((kvp) =>
					{
						int idx = kvp.Value;
						CreateCarrier("HW_YouAre", (outSignal) =>
							{
								outSignal.ID = kvp.Key;
								outSignal.CaveNumber = idx;
								outSignal.AdjoiningCave1 = caveMatrix[idx][0];
								outSignal.AdjoiningCave2 = caveMatrix[idx][1];
								outSignal.AdjoiningCave3 = caveMatrix[idx][2];
								outSignal.HasBottomlessPit = pits.Contains(idx);
								outSignal.HasSuperBats = bats.Contains(idx);
								outSignal.HasWumpus = wumpus == idx;
								outSignal.HasPlayer = player == idx;
							});
					});

				rsys.Remove(this);
			}
		}

		// Pits can go anywhere except twice or thrice in the same cave.
		protected void AssignPits()
		{
			// Probably a better way to do this.
			pits.Add(rnd.Next(20));
			List<int> available = caveNumbers.Where(cn => !pits.Contains(cn)).ToList();
			pits.Add(available[rnd.Next(19)]);
			available = caveNumbers.Where(cn => !pits.Contains(cn)).ToList();
			pits.Add(available[rnd.Next(18)]);
		}

		// Bats can go anywhere except twice or thrice in the same cave.
		protected void AssignBats()
		{
			bats.Add(rnd.Next(20));
			List<int> available = caveNumbers.Where(cn => !bats.Contains(cn)).ToList();
			bats.Add(available[rnd.Next(19)]);
			available = caveNumbers.Where(cn => !bats.Contains(cn)).ToList();
			bats.Add(available[rnd.Next(18)]);
		}

		// The Wumpus can go anywhere.
		protected void AssignWumpus()
		{
			wumpus = rnd.Next(20);
		}

		// The player must start off in a location where he/she can live.
		protected void AssignPlayer()
		{
			List<int> available = caveNumbers.Where(cn => (cn != wumpus) && !bats.Contains(cn) && !pits.Contains(cn)).ToList();
			player = available[rnd.Next(available.Count)];
		}
	}
}

