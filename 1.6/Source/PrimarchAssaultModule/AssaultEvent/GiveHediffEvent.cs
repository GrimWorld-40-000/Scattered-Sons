using System;
using RimWorld;
using Verse;

namespace PrimarchAssault.AssaultEvent
{
	public class GiveHediffEventProperties : AssaultEventActionProperties
	{
		public float severity = 1;
		public HediffDef def;
		public override Type AssaultEventClass() => typeof(GiveHediffEvent);
	}
    
	public class GiveHediffEvent: AssaultEventAction
	{
		private GiveHediffEventProperties Props => (GiveHediffEventProperties) props;
		public override void Apply(Map map)
		{
			base.Apply(map);
			
			if (!TryGetSpawnedChampion(out Pawn champion)) return;

			Hediff diff = HediffMaker.MakeHediff(Props.def, champion);
			diff.Severity = Props.severity;
			champion.health.AddHediff(diff);
		}
	}
}