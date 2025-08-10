using System;
using System.Collections.Generic;
using System.Linq;
using PrimarchAssault.External;
using PrimarchAssault.Jobs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace PrimarchAssault
{
	public class LordJob_Duel: LordJob_Champion
	{
		public LordJob_Duel(Pawn duelist, List<Pawn> guards) : base(duelist, guards)
		{
			point = duelist.PositionHeld;
			// if (TryGetDuelSpawnCenter(, out IntVec3 cell)
			// {
			// 	
			// }
			//point = CellFinder
		}

		public LordJob_Duel() {}

		public IntVec3 point;
		private float? wanderRadius;
		private float? defendRadius;
		private bool isCaravanSendable;
		private static float DuelStartRadius = 5;
		private HostilityResponseMode startingResponseMode = HostilityResponseMode.Flee;
		private int endAfterTick = -1;
		private bool isDuelDisrespected = false;
		
		
		//Duel stuff imported from Ideo duel
		private Pawn playerDuelist;
		private bool duelStarted;
		private int attacksThisStage;
		private int movingTicks;
		private bool duelFinished;
		private static readonly IntRange AttacksPerStage = new IntRange(4, 8);
		private static readonly IntRange MovingTicksPerStage = new IntRange(360, 600);

		public override bool IsCaravanSendable => isCaravanSendable;

		public PrimarchDuelBehaviorStage CurrentDuelStage
		{
			get => duelFinished
				? PrimarchDuelBehaviorStage.BackOff
				: attacksThisStage <= 0 ? PrimarchDuelBehaviorStage.Move : PrimarchDuelBehaviorStage.Attack;
		}

		public override bool NeverInRestraints => true;


		public IEnumerable<Pawn> GetDuelists()
		{
			yield return Champion;
			if (playerDuelist != null)
				yield return playerDuelist;
		}
		
		public override bool ShouldRemovePawn(Pawn p, PawnLostCondition reason)
		{
			return reason != PawnLostCondition.Incapped && base.ShouldRemovePawn(p, reason);
		}
		
		public void StartDuelIfNotStartedYet()
		{
			if (duelStarted)
				return;

			//Make champion unkillable by normal means
			GetChampionHediff().CanBeKilled = false;
			
			foreach (Pawn pawn in GetDuelists())
			{
				if (pawn.drafter != null)
					pawn.drafter.Drafted = false;
			}
			
			
			duelStarted = true;
			StartDuel();
		}
		
		private void InterruptDuelistJobs()
		{
			foreach (Pawn duelist in GetDuelists())
				duelist.jobs?.CheckForJobOverride();
		}
		
		private void StartDuel() => StartMoving();

		private void StartMoving()
		{
			attacksThisStage = 0;
			movingTicks = MovingTicksPerStage.RandomInRange;
			InterruptDuelistJobs();
		}
		private void StartAttacking()
		{
			movingTicks = 0;
			attacksThisStage = AttacksPerStage.RandomInRange;
			InterruptDuelistJobs();

			startingResponseMode = playerDuelist.playerSettings.hostilityResponse;
			playerDuelist.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
		}

		private void EndDuel()
		{
			if (!playerDuelist.Dead)
			{
				playerDuelist.playerSettings.hostilityResponse = startingResponseMode;
				playerDuelist.mindState.mentalStateHandler.Reset();
			}
			
			
			List<Pawn> pawns = lord.ownedPawns.ToList();
			// lord.RemoveAllPawns();
				
			foreach (Pawn lordPawn in pawns)
			{
				if (lordPawn != playerDuelist && lordPawn != Champion)
				{
					EffecterDefOf.Skip_EntryNoDelay.Spawn(lordPawn, lordPawn.MapHeld).Cleanup();
					lordPawn.DeSpawn();
				}
			}
			
			GetChampionHediff().FinalizeAndRemove(true, isDuelDisrespected);
				
			lord.Notify_SignalReceived(new Signal(lord.inSignalLeave));
		}
		
		private void EndDuelHostile()
		{
			if (!playerDuelist.Dead)
			{
				playerDuelist.playerSettings.hostilityResponse = startingResponseMode;
				playerDuelist.mindState.mentalStateHandler.Reset();
			}
			
			
			List<Pawn> pawns = lord.ownedPawns.ToList();
			lord.RemoveAllPawns();
			pawns.Remove(playerDuelist);

			LordMaker.MakeNewLord(Champion.Faction, new LordJob_AssaultColony(Champion.Faction, false, false),
				Champion.MapHeld, pawns);
			
			Find.LetterStack.ReceiveLetter("GWPA.DuelDisrespectedTitle".Translate(), "GWPA.DuelDisrespected".Translate(), LetterDefOf.NegativeEvent);	
			
			lord.Notify_SignalReceived(new Signal(lord.inSignalLeave));
			
			
		}

		public override void LordJobTick()
		{
			base.LordJobTick();

			if (duelStarted)
			{
				if (endAfterTick != -1 && endAfterTick < Find.TickManager.TicksGame)
				{
					EndDuel();
					return;
				}
			
			
			
				Find.TickManager.slower.SignalForceNormalSpeedShort();

				foreach (Pawn duelist in GetDuelists())
				{
					if (duelist.mindState != null)
					{
						if (duelist.mindState.lastAttackTargetTick == Find.TickManager.TicksGame)
						{
							NotifyMeleeAttack();
							if (attacksThisStage <= 0)
							{
								StartMoving();
								return;
							}
						}
					}
				}
			}
			
				
			
			if (movingTicks <= 0)
				return;
			--movingTicks;
			if (movingTicks > 0)
				return;
			StartAttacking();
		}

		public void Notify_DamageTaken(DamageInfo dinfo)
		{
			if (dinfo.Instigator != playerDuelist)
			{
				isDuelDisrespected = true;
				EndDuelHostile();
			}
		}

		public Pawn GetDuelistPawn()
		{
			if (playerDuelist == null)
			{
				if (!FindDuelist())
				{
					Log.Error("Couldn't find a duelist.");
				}
			}
			return playerDuelist;
		}

		private Hediff_Champion GetChampionHediff()
		{
			if (Champion.health.hediffSet.TryGetHediff(PADefsOf.GWPA_Champion, out Hediff champion))
			{
				return (Hediff_Champion)champion;
			}

			return null;
		}
		
		public Pawn Opponent(Pawn duelist)
		{
			return GetDuelists().ToList()[GetDuelists().ToList().IndexOf(duelist) == 0 ? 1 : 0];
		}

		public override void Notify_PawnDowned(Pawn p)
		{
			if (p != Champion && p!= playerDuelist)
			{
				//Guard dies
				isDuelDisrespected = true;
				duelFinished = true;
				endAfterTick = Find.TickManager.TicksGame + 240;
				InterruptDuelistJobs();
			}
		}

		public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
		{
			if (p == playerDuelist)
			{
				//Player dies
			}
			else if (p != Champion)
			{
				//Guard dies
				isDuelDisrespected = true;
			}
			else
			{
				return;
			}
			//When player or a guard dies
			duelFinished = true;
			endAfterTick = Find.TickManager.TicksGame + 240;
			InterruptDuelistJobs();
		}
		

		private void NotifyMeleeAttack()
		{
			--attacksThisStage;
			//Check if Primarch is Low
			if (GetChampionHediff().HealthPercent <= 0.0f && !duelFinished)
			{
				duelFinished = true;
				endAfterTick = Find.TickManager.TicksGame + 240;
				InterruptDuelistJobs();
				DropPrimarchRewards();
			}
		}

		private void DropPrimarchRewards()
		{
			List<ThingDefCountClass> things = GetChampionHediff().GetDroppedThings();
			if (!things.Empty())
			{
				ThingDefCountClass thing = things.RandomElement();

				
				ChallengeUtilities.RandomNotOwnedThingFromList(things.Select(thingDefClass => thingDefClass.thingDef).ToList(), Map);
				
				ThingDef stuff = null;

				if (thing.thingDef.MadeFromStuff)
				{
					stuff = DefDatabase<ThingDef>.GetNamed("HP_Adamantium");
				}
				            
				GenPlace.TryPlaceThing(ThingMaker.MakeThing(thing.thingDef, stuff), Champion.Position, Champion.Map, ThingPlaceMode.Near);
			}
		}
		
		
		public override bool BlocksSocialInteraction(Pawn pawn) => GetDuelists().Contains(pawn);

		
		
		
		
		
		
		
		

		
		public override StateGraph CreateGraph()
		{
			StateGraph graph = new StateGraph();
			//point

			LordToil_IdleBeforeDuel idle = new LordToil_IdleBeforeDuel(Guards, Champion);
			graph.AddToil(idle);
			LordToil_GuardDuel guard = new LordToil_GuardDuel(Guards, Champion);
			graph.AddToil(guard);
			LordToil_PrimarchDuel duel = new LordToil_PrimarchDuel(Guards, Champion);
			graph.AddToil(duel);
			//new LordToil_DefendPoint(point, defendRadius, wanderRadius)


			Transition beginGuarding = new Transition(idle, guard);
			beginGuarding.AddTrigger(new Trigger_HostilePawnNearby());
			graph.AddTransition(beginGuarding);
			
			
			Transition beginDueling = new Transition(guard, duel);
			beginDueling.AddTrigger(new Trigger_ValidDuelistNear(point, DuelStartRadius));
			graph.AddTransition(beginDueling);
			
			
			
			return graph;
		}

		public bool FindDuelist()
		{
			foreach (Pawn pawn1 in lord.Map.mapPawns.AllHumanlikeSpawned)
			{
				if (pawn1.health.hediffSet.TryGetHediff(PADefsOf.GWPA_Duelist, out Hediff _))
				{
					playerDuelist = pawn1;
					playerDuelist.mindState.mentalStateHandler.TryStartMentalState(PADefsOf.GWPA_Dueling);
					
					if (!lord.ownedPawns.Contains(playerDuelist))
						lord.AddPawn(playerDuelist);
					return true;
				}
			}
			return false;
		}
		
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref point, "point");
			Scribe_Values.Look(ref wanderRadius, "wanderRadius");
			Scribe_Values.Look(ref defendRadius, "defendRadius");
			Scribe_Values.Look(ref isCaravanSendable, "isCaravanSendable");
			Scribe_References.Look(ref playerDuelist, "duelists");
			Scribe_Values.Look(ref movingTicks, "movingTicks");
			Scribe_Values.Look(ref attacksThisStage, "attacksThisStage");
			Scribe_Values.Look(ref duelStarted, "duelStarted");
			Scribe_Values.Look(ref startingResponseMode, "startingResponseMode");
			Scribe_Values.Look(ref endAfterTick, "endAfterTick");
			Scribe_Values.Look(ref isDuelDisrespected, "isDuelDisrespected");
		}
	}

	public enum PrimarchDuelBehaviorStage
	{
		Attack,
		Move,
		BackOff
	}
}