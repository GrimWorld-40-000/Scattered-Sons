using PrimarchAssault.GameConditions;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PrimarchAssault.External
{
	public class HediffComp_PsychicStormSuppression: HediffComp
	{
		public override bool CompShouldRemove
		{
			get
			{
				if (!Pawn.SpawnedOrAnyParentSpawned) return true;
				PsychicStorm activeCondition = Pawn.MapHeld.gameConditionManager.GetActiveCondition<PsychicStorm>();
				return activeCondition == null;
			}
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			base.CompPostTick(ref severityAdjustment);

			if (!parent.pawn.IsHashIntervalTick(10)) return;
			if (parent.pawn.HostileTo(Faction.OfPlayer)) return;
			if (!ModLister.RoyaltyInstalled) return;
			if (parent.pawn.HasPsylink)
			{
				parent.pawn.psychicEntropy?.TryAddEntropy(0.3f, overLimit: true);
			}
		}
	}
}