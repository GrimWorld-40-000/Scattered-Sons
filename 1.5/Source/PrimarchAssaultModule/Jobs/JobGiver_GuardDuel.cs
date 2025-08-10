using RimWorld;
using Verse;
using Verse.AI;

namespace PrimarchAssault.Jobs
{
	public class JobGiver_GuardDuel : ThinkNode_JobGiver
	{
		
		protected override Job TryGiveJob(Pawn pawn)
		{
			Pawn duelist = pawn.mindState.duty.focus.Pawn;
			if (duelist != null)
			{
				//TODO make this in a circle area
				if (tryFindGuardCell(duelist.PositionHeld, pawn.PositionHeld, duelist.MapHeld, out IntVec3 cell))
				{
					return JobMaker.MakeJob(PADefsOf.GWPA_GuardDuel, cell, pawn.mindState.duty.focus.Pawn);
				}
				return JobMaker.MakeJob(JobDefOf.Wait_Wander);
			}
			else
			{
				return JobMaker.MakeJob(JobDefOf.Wait_Wander);
			}
		}

		protected bool tryFindGuardCell(IntVec3 duelistPosition, IntVec3 guardPosition, Map map, out IntVec3 cell)
		{

			return CellFinder.TryFindRandomCellNear(duelistPosition, map, 9, vec3 =>
				{
					float distance = vec3.DistanceToSquared(duelistPosition);
					return distance is > 64 and < 90 && map.reachability.CanReach(guardPosition,
						vec3,
						PathEndMode.OnCell,
						TraverseMode.NoPassClosedDoors);
				}
				, out cell);


			// CellFinder.TryFindRandomEdgeCellNearWith(duelist.PositionHeld, 9, duelist.MapHeld,
			// 	vec3 => , out IntVec3 cell)
		}
	}
}