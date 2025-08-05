using System.Collections.Generic;
using PrimarchAssault.External;
using RimWorld;
using Verse;
using AbilityDef = RimWorld.AbilityDef;

namespace PrimarchAssault
{
    public abstract class ChampionStage: IExposable
    {
        public float stage = -1;

        public abstract void Apply(Pawn pawn, Map map);

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref stage, "stage");
        }
    }
    
    public class ChampionAbilityStage: ChampionStage
    {
        public List<AbilityDef> abilitiesToGain;
        public override void Apply(Pawn pawn, Map map)
        {
            foreach (AbilityDef ability in abilitiesToGain)
            {
                pawn.abilities.GainAbility(ability);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref abilitiesToGain, "abilitiesToGain", LookMode.Def);
        }
    }
    
    
    
    public class ChampionEventStage: ChampionStage
    {
        public AssaultEventDef assaultEventDef;
        public bool triggerOnAssaultStart = false;
        public bool triggerOnChampionArrive = false;
        public bool triggerOnChampionKilled = false;
        
        //If stage is negative it will never fire normally
        public override void Apply(Pawn pawn, Map map)
        {
            assaultEventDef.FireAllComps(map);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref assaultEventDef, "assaultEvent");
        }
    }
    
    public class ChampionHediffStage: ChampionStage
    {
        public List<HediffDef> hediffsToGain;
        public override void Apply(Pawn pawn, Map map)
        {
            foreach (HediffDef hediffDef in hediffsToGain)
            {
                pawn.health.AddHediff(hediffDef);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref hediffsToGain, "hediffsToGain", LookMode.Def);
        }
    }
}