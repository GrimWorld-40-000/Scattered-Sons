using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PrimarchAssault.GameConditions
{
	public class PsychicStorm : GameCondition
	{
		private static void CheckPawn(Pawn pawn)
		{
			if (!pawn.RaceProps.Humanlike || pawn.health.hediffSet.HasHediff(PADefsOf.GPWA_PsychicStormSuppression))
				return;
			pawn.health.AddHediff(PADefsOf.GPWA_PsychicStormSuppression);
		}

		public override void GameConditionTick()
		{
			foreach (var t in AffectedMaps.Select(affectedMap => affectedMap.mapPawns.AllPawns).SelectMany(allPawns => allPawns))
			{
				CheckPawn(t);
			}
		}
	}
}