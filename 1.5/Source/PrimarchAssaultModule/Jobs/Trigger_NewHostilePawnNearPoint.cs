using RimWorld;
using Verse;
using Verse.AI.Group;

namespace PrimarchAssault.Jobs
{
	public class Trigger_NewHostilePawnNearPoint: Trigger
	{
		private const int CheckInterval = 40;
		
		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			return signal.type == TriggerSignalType.Tick && Find.TickManager.TicksGame % CheckInterval == 0 &&
			       AnyHostileNear(lord);
		}

		private IntVec3 point;
		private float distanceSquared;

		public Trigger_NewHostilePawnNearPoint(IntVec3 point, float distance)
		{
			distanceSquared = distance * distance;
			this.point = point;
		}
		
		private bool AnyHostileNear(Lord lord)
		{
			foreach (Pawn pawn1 in lord.Map.mapPawns.AllHumanlikeSpawned)
			{
				if (pawn1 != null)
				{
					if (pawn1.HostileTo(lord.faction) && IsPawnValid(pawn1))
					{
						if (point.DistanceToSquared(pawn1.PositionHeld) < distanceSquared)
						{
							pawn1.health.AddHediff(PADefsOf.GWPA_Duelist);
							return true;
						}
					}
				}
			}

			return false;
		}

		public virtual bool IsPawnValid(Pawn candidate)
		{
			return true;
		}
	}
}