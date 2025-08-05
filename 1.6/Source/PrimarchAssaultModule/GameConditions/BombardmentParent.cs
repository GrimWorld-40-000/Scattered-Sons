using RimWorld;
using Verse;

namespace PrimarchAssault.GameConditions
{
	public abstract class BombardmentParent: GameCondition
	{
		protected virtual int IntervalTicks => 300;
		protected IntVec3 NextTargetCell;
		protected int NextTriggerTick;
		
		
		public override bool AllowEnjoyableOutsideNow(Map map) => false;

		public override void ExposeData()
		{
			base.ExposeData();
			
			Scribe_Values.Look(ref NextTriggerTick, "nextTriggerTic");
		}

		public override void PostMake()
		{
			base.PostMake();
			NextTriggerTick = GenTicks.TicksGame + IntervalTicks;
		}

		protected virtual void Trigger()
		{
			NextTriggerTick = GenTicks.TicksGame + IntervalTicks;
			FindValidTargetingPoint();
		}

		public override void GameConditionTick()
		{
			base.GameConditionTick();
			
			int ticksGame = GenTicks.TicksGame;
			
			if (NextTriggerTick <= ticksGame && TicksLeft >= IntervalTicks)
			{
				Trigger();
			}
		}


		private void FindValidTargetingPoint()
		{
			IntVec3 cell;

			do
			{
				cell = CellFinderLoose.RandomCellWith(x => x.InBounds(SingleMap) && !x.Roofed(SingleMap), SingleMap);
			} while (!cell.IsValid);

			NextTargetCell = cell;
		}
	}
}