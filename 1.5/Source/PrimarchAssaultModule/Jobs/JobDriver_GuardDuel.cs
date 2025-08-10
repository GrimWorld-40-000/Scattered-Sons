using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace PrimarchAssault.Jobs
{
	public class JobDriver_GuardDuel : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil walkTo = Toils_Goto.GotoCell(job.targetA.Cell, PathEndMode.OnCell);
			yield return walkTo;
			
			
			Toil stand = Toils_General.Wait(int.MaxValue);
			stand.tickAction = () =>
			{
				pawn.rotationTracker.FaceTarget(job.targetB);
				pawn.GainComfortFromCellIfPossible();
				Pawn actor = stand.actor;
				if (!actor.IsHashIntervalTick(100))
					return;
				actor.jobs.CheckForJobOverride();
			};
			//stand.socialMode = RandomSocialMode.Off;
			stand.defaultCompleteMode = ToilCompleteMode.Never;
			stand.handlingFacing = true;
			yield return stand;
		}
	}
}