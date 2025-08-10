using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace PrimarchAssault.External
{
	public class LordToil_WanderCloseIgnoreEnemies: LordToil
	{
		private IntVec3 location;

		public LordToil_WanderCloseIgnoreEnemies(IntVec3 location) => this.location = location;

		public override void UpdateAllDuties()
		{
			for (int index = 0; index < lord.ownedPawns.Count; ++index)
			{
				PawnDuty pawnDuty = new PawnDuty(PADefsOf.GWPA_WanderCloseIgnoreEnemies, location);
				lord.ownedPawns[index].mindState.duty = pawnDuty;
			}
		}
	}
}