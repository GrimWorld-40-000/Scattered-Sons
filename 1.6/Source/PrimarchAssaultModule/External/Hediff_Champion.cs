using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using PrimarchAssault;
using Verse;
using Verse.AI;

namespace PrimarchAssault.External
{
    public class Hediff_Champion: Hediff
    {
        private List<ChampionStage> _stages = new List<ChampionStage>();
        private List<ThingDefCountClass> _droppedThings;
        private ChallengeDef _challenge;
        private float _currentHp;
        public bool CanBeKilled = true;



        private List<ChampionStage> _tmpStagesToRemove = new List<ChampionStage>();
        public void SetupHediff(List<ThingDefCountClass> droppedThings, List<ChampionStage> stages, ChallengeDef challenge)
        {
            _droppedThings = droppedThings;
            _stages = stages;
            _challenge = challenge;
            _currentHp = challenge.championHp;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _stages, "stages", LookMode.Deep);
            Scribe_Collections.Look(ref _droppedThings, "droppedThings", LookMode.Deep);
            Scribe_Defs.Look(ref _challenge, "challenge");
            Scribe_Values.Look(ref _currentHp, "currentHp");
            Scribe_Values.Look(ref CanBeKilled, "canBeKilled");
        }

        public List<ThingDefCountClass> GetDroppedThings()
        {
	        return _droppedThings;
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);

            //Trigger all on-kill effects
            _stages?.Where(stage => stage is ChampionEventStage { triggerOnChampionKilled: true }).Do(stage => stage.Apply(pawn, pawn.Corpse.Map)) ;


            if (pawn.MapHeld != null)
            {
	            if (GameComponent_ChallengeManager.Instance.ConditionsCreatedByEvent.ContainsKey(pawn.MapHeld.info.Tile))
	            {
		            foreach (GameCondition condition in GameComponent_ChallengeManager.Instance.ConditionsCreatedByEvent[pawn.MapHeld.info.Tile].SelectMany(conditionDef => pawn.MapHeld.gameConditionManager.ActiveConditions.Where(condition => condition.def == conditionDef)))
		            {
			            condition.End();
		            }
	            }
            }
            
            
            
            if (_droppedThings != null)
            {
	            
	            List<ThingDefCountClass> things = _droppedThings;
	            if (!things.Empty())
	            {
		            ThingDefCountClass thing = things.RandomElement();

				
		            ChallengeUtilities.RandomNotOwnedThingFromList(things.Select(thingDefClass => thingDefClass.thingDef).ToList(), pawn.MapHeld);
				
		            ThingDef stuff = null;

		            if (thing.thingDef.MadeFromStuff)
		            {
			            stuff = DefDatabase<ThingDef>.GetNamed("HP_Adamantium");
		            }
				            
		            GenPlace.TryPlaceThing(ThingMaker.MakeThing(thing.thingDef, stuff), pawn.Position, pawn.MapHeld, ThingPlaceMode.Near);
	            }
	            // foreach (ThingDefCountClass droppedThing in _droppedThings)
	            // {
		           //  // Thing thing = ThingMaker.MakeThing(droppedThing.thingDef);
		           //  // thing.stackCount = 
		           //  for (int i = 0; i < droppedThing.count; i++)
		           //  {
			          //   ThingDef stuff = null;
	            //
			          //   if (droppedThing.thingDef.MadeFromStuff)
			          //   {
				         //    stuff = DefDatabase<ThingDef>.GetNamed("HP_Adamantium");
			          //   }
				         //    
			          //   GenPlace.TryPlaceThing(ThingMaker.MakeThing(droppedThing.thingDef, stuff), pawn.Position, pawn.Corpse.Map, ThingPlaceMode.Near);
		           //  }
	            // }

            }

            
            FinalizeAndRemove();
        }

        public void FinalizeAndRemove(bool forDuel = false, bool isDuelDisrespected = false)
        {
	        if (forDuel)
	        {
		        if (isDuelDisrespected)
		        {
			        Find.LetterStack.ReceiveLetter("GWPA.DuelDisrespectedTitle".Translate(), "GWPA.DuelDisrespected".Translate(), LetterDefOf.NegativeEvent);
		        }
		        else if (pawn.Dead)
		        {
			        Find.LetterStack.ReceiveLetter("GWPA.DuelWonTitle".Translate(), "GWPA.DuelWon".Translate(), LetterDefOf.PositiveEvent);
		        }
		        else
		        {
			        Find.LetterStack.ReceiveLetter("GWPA.DuelFailedTitle".Translate(), "GWPA.DuelFailed".Translate(), LetterDefOf.PositiveEvent);
		        }
	        }
	        else
	        {
		        //Get rid of the corpse
		        if (pawn.Dead)
		        {
			        Find.LetterStack.ReceiveLetter("GWPA.GivenUpTitle".Translate(), "GWPA.GivenUp".Translate(), LetterDefOf.PositiveEvent);
		        }
		        else
		        {
			        Find.LetterStack.ReceiveLetter("GWPA.GivenUpTitle".Translate(), "GWPA.GivenUp".Translate(), LetterDefOf.PositiveEvent);
		        }
	        }
	        
	        
	        if (pawn.Dead)
	        {
		        EffecterDefOf.Skip_EntryNoDelay.Spawn(pawn.Corpse, pawn.Corpse.MapHeld).Cleanup();
		        pawn.Corpse.DeSpawn();
	        }
	        else
	        {
		        EffecterDefOf.Skip_EntryNoDelay.Spawn(pawn, pawn.MapHeld).Cleanup();
		        pawn.DeSpawn();
	        }
            
	        GameComponent_ChallengeManager.Instance.RemoveActiveChampion(pawn.thingIDNumber);
        }

        public void DamageHealthBar(float amount, DamageInfo dinfo)
        {
	        _currentHp -= amount;

	        if (pawn.lord.LordJob is LordJob_Duel duel)
	        {
		        duel.Notify_DamageTaken(dinfo);
	        }
        }

        public override void Tick()
        {
            base.Tick();
            
            float percent = GetChampionStage(out float shieldValue, out float healthValue);

            if (pawn.TryGetComp(out CompGlower glower))
            {
                glower.UpdateLit(pawn.Map);
            }
            
            if (pawn.TryGetComp(out CompShield shield))
            {
                shieldValue = shield.Energy / pawn.GetStatValue(StatDefOf.EnergyShieldEnergyMax);
            }
            
            GameComponent_ChallengeManager.Instance.HealthBar.UpdateIfWilling(pawn.thingIDNumber, healthValue, shieldValue, _challenge.healthBarRelative, _challenge.shieldBarRelative);
            
            if (_currentHp <= 0)
            {
	            if (CanBeKilled)
	            {
		            pawn.inventory.DestroyAll();
		            pawn.equipment.DestroyAllEquipment();
		            pawn.Kill(null);
	            }
	            return;
            }
            
            
            if (!pawn.IsHashIntervalTick(100)) return;
            
            _tmpStagesToRemove.Clear();
            foreach (var stage in _stages.Where(stage => stage.stage > percent))
            {
                stage.Apply(pawn, pawn.Map);
                _tmpStagesToRemove.Add(stage);
            }

            _stages.RemoveAll(stage => _tmpStagesToRemove.Contains(stage));
        }
        
        public float HealthPercent => _currentHp / _challenge.championHp;


        private float GetChampionStage(out float apparelValue, out float healthValue)
        {
            apparelValue = (float)pawn.apparel.WornApparel.Select(apparel => apparel.HitPoints / (double)apparel.MaxHitPoints).Average();
            healthValue = HealthPercent;
            
            return Math.Min(apparelValue, healthValue);
        }
    }

}