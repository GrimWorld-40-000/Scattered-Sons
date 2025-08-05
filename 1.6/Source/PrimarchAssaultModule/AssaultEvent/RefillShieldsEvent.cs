using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PrimarchAssault.AssaultEvent
{
	public class RefillShieldsEventProperties : AssaultEventActionProperties
	{
		public override Type AssaultEventClass() => typeof(RefillShieldsEvent);
	}
	
	public class RefillShieldsEvent: AssaultEventAction
	{
		protected override IEnumerable<TargetInfo> GetTargets()
		{
			if (TryGetSpawnedChampion(out Pawn champion)) 
				yield return champion;
		}

		public override void Apply(Map map)
		{
			base.Apply(map);
			if (!TryGetSpawnedChampion(out Pawn champion)) return;
			if (!champion.TryGetComp(out CompShield shield)) return;
			Traverse.Create(shield).Method("Reset").GetValue();
			Traverse.Create(shield).Field("energy").SetValue(shield.parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax));
		}
		
	}
}