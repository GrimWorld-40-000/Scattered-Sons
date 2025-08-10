using System;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace PrimarchAssault.Jobs
{
	public class JobGiver_PrimarchDuel : JobGiver_AIFightEnemies
	{
		public const float MinDistOpponentWhenMoving = 1.9f;
		public const float MaxFightMoveDist = 3.1f;

		protected override bool DisableAbilityVerbs => true;

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!(pawn.GetLord()?.LordJob is LordJob_Duel lordJob))
				return null;
			lordJob.StartDuelIfNotStartedYet();
			if (lordJob.CurrentDuelStage == PrimarchDuelBehaviorStage.Attack)
				return base.TryGiveJob(pawn);
			if (lordJob.CurrentDuelStage == PrimarchDuelBehaviorStage.BackOff)
			{
				// Job backOffJob = JobMaker.MakeJob(JobDefOf.Wait_MaintainPosture);
				// backOffJob.checkOverrideOnExpire = true;
				// backOffJob.reactingToMeleeThreat
				// backOffJob.expiryInterval = 240;
				// backOffJob.collideWithPawns = true;
				// backOffJob.locomotionUrgency = LocomotionUrgency.Amble;
				
				// return backOffJob;
				Job backOffJob = JobMaker.MakeJob(JobDefOf.Goto, GetMoveTarget(pawn, lordJob), (LocalTargetInfo) (Thing) lordJob.Opponent(pawn));
				backOffJob.checkOverrideOnExpire = true;
				backOffJob.expiryInterval = 240;
				backOffJob.collideWithPawns = true;
				backOffJob.locomotionUrgency = LocomotionUrgency.Amble;
				return backOffJob;
			}
			Job job = JobMaker.MakeJob(JobDefOf.Goto, GetMoveTarget(pawn, lordJob), (LocalTargetInfo) (Thing) lordJob.Opponent(pawn));
			job.checkOverrideOnExpire = true;
			job.expiryInterval = 40;
			job.collideWithPawns = true;
			job.locomotionUrgency = LocomotionUrgency.Sprint;
			return job;
		}

		private LocalTargetInfo GetMoveTarget(Pawn pawn, LordJob_Duel duel)
		{
			Pawn opponent = duel.Opponent(pawn);
			return RCellFinder.RandomWanderDestFor(pawn, duel.point, 3.1f, (p, c, r) =>
			{
				if (c == pawn.Position || !c.Standable(p.Map) ||
				    !p.CanReserveAndReach(c, PathEndMode.OnCell, Danger.Deadly) ||
				    c.DistanceTo(duel.point) > 3.0999999046325684)
					return false;
				IntVec3 a1 = opponent.CurJob?.def == JobDefOf.Goto ? opponent.CurJob.targetA.Cell : IntVec3.Invalid;
				if (c.DistanceTo(opponent.Position) < 1.899999976158142 ||
				    a1 != IntVec3.Invalid && a1.DistanceTo(c) < 1.899999976158142)
					return false;
				PawnPath path = pawn.Map.pathFinder.FindPath(pawn.Position, c, pawn);
				try
				{
					foreach (IntVec3 a2 in path.NodesReversed)
					{
						if (a2.DistanceTo(opponent.Position) < 1.899999976158142 ||
						    a2.DistanceTo(duel.point) > 3.0999999046325684)
							return false;
					}

					if (a1 != IntVec3.Invalid)
					{
						if (opponent.pather.curPath != null)
						{
							foreach (IntVec3 a3 in opponent.pather.curPath.NodesReversed)
							{
								if (a3.DistanceTo(pawn.Position) < 1.899999976158142 ||
								    a3.DistanceTo(duel.point) > 3.0999999046325684)
									return false;
								foreach (IntVec3 b in path.NodesReversed)
								{
									if (a3.DistanceTo(b) < 1.899999976158142)
										return false;
								}
							}
						}
					}
				}
				finally
				{
					path.ReleaseToPool();
				}

				return true;
			}, Danger.Deadly);
		}

		protected override void UpdateEnemyTarget(Pawn pawn)
		{
			Pawn pawn1 = ((LordJob_Duel) pawn.GetLord().LordJob).Opponent(pawn);
			if (pawn1 == null || pawn1.Dead)
				pawn.mindState.enemyTarget = null;
			else
				pawn.mindState.enemyTarget = pawn1;
		}

		protected override Job MeleeAttackJob(Pawn pawn, Thing enemyTarget)
		{
			Job job = base.MeleeAttackJob(pawn, enemyTarget);
			job.killIncappedTarget = true;
			return job;
		}
	}
}