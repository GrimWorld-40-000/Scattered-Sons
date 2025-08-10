using RimWorld;
using Verse;

namespace PrimarchAssault.Jobs
{
	public class Trigger_ValidDuelistNear: Trigger_NewHostilePawnNearPoint
	{
		public Trigger_ValidDuelistNear(IntVec3 point, float distance) : base(point, distance)
		{
		}

		public override bool IsPawnValid(Pawn candidate)
		{
			//Must be alive and well
			if (candidate.DeadOrDowned)
			{
				return false;
			}
			//Must be player controlled
			if (!candidate.IsPlayerControlled)
			{
				return false;
			}
			//Must be permanent colonist
			if (candidate.GetExtraHostFaction() != null)
			{
				return false;
			}
			//Must not be slave
			if (candidate.IsSlave)
			{
				return false;
			}
			//Must be capable of violence
			if (candidate.WorkTagIsDisabled(WorkTags.Violent))
			{
				return false;
			}
			//Cannot have won a duel in the past
			if (candidate.health.hediffSet.HasHediff(PADefsOf.GWPA_DuelVictor))
			{
				return false;
			}

			return true;
		}
	}
}