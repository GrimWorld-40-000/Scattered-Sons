using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace PrimarchAssault.GameConditions
{
	public class LaserBombardment: BombardmentParent
	{
		protected override int IntervalTicks => 1500;

		/*private const float Radius = 7.5f;
		private const int FiresStartedPerTick = 3;
		private static readonly IntRange FlameDamageAmountRange = new IntRange(30, 60);
		private static readonly IntRange CorpseFlameDamageAmountRange = new IntRange(5, 10);
		private static readonly List<Thing> TMPThings = new List<Thing>();*/
		private bool _onRightNow;


		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref _onRightNow, "onRightNow");
		}


		protected override void Trigger()
		{
			//Toggle
			_onRightNow = !_onRightNow;
			base.Trigger();
			if (!_onRightNow) return;
			//MoteMaker.MakePowerBeamMote(NextTargetCell, SingleMap);
			PowerBeam powerBeam = (PowerBeam) GenSpawn.Spawn(ThingDefOf.PowerBeam, NextTargetCell, SingleMap);
			powerBeam.duration = 600;
			powerBeam.StartStrike();
		}
		
		
		/*public override void GameConditionTick()
		{
			base.GameConditionTick();

			if (_onRightNow)
			{
				for (int index = 0; index < FiresStartedPerTick; ++index) StartRandomFireAndDoFlameDamage(); 
			}
		}
		
		
		private void StartRandomFireAndDoFlameDamage()
		{
			IntVec3 c = GenRadial.RadialCellsAround(NextTargetCell, Radius, true).Where(x => x.InBounds(SingleMap)).RandomElementByWeight(x => (float) (1.0 - Mathf.Min(x.DistanceTo(NextTargetCell) / 15f, 1f) + 0.05f));
			FireUtility.TryStartFireIn(c, SingleMap, Rand.Range(0.1f, 0.925f), null);
			TMPThings.Clear();
			TMPThings.AddRange(c.GetThingList(SingleMap));
			foreach (var thing in TMPThings)
			{
				IntRange damageAmountRange;
				int randomInRange;
				if (thing is not Corpse)
				{
					damageAmountRange = FlameDamageAmountRange;
					randomInRange = damageAmountRange.RandomInRange;
				}
				else
				{
					damageAmountRange = CorpseFlameDamageAmountRange;
					randomInRange = damageAmountRange.RandomInRange;
				}
				int amount = randomInRange;
				Pawn tmpThing = thing as Pawn;
				BattleLogEntry_DamageTaken entryDamageTaken = null;
				if (tmpThing != null)
				{
					entryDamageTaken = new BattleLogEntry_DamageTaken(tmpThing, RulePackDefOf.DamageEvent_PowerBeam, null);
					Find.BattleLog.Add(entryDamageTaken);
				}
				thing.TakeDamage(new DamageInfo(DamageDefOf.Flame, amount, instigator: null, weapon: null)).AssociateWithLog(entryDamageTaken);
			}
			TMPThings.Clear();
		}*/
	}
}