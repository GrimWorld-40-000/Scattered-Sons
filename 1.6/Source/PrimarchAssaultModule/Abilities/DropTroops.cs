using System.Collections.Generic;
using System.Linq;
using RimWorld;
using PrimarchAssault;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace PrimarchAssault.Abilities
{
    public class CompProperties_DropTroops: AbilityCompProperties
    {
        public List<PawnKindDef> pawnKinds;
        public int combatScore;
        
        public CompProperties_DropTroops()
        {
            compClass = typeof(CompAbilityEffect_DropTroops);
        }
    }
    
    public class CompAbilityEffect_DropTroops: CompAbilityEffect
    {
        
        private CompProperties_DropTroops Props => (CompProperties_DropTroops)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            
            List<Pawn> pawnsToGenerate = CreateWave(parent.pawn.Faction).ToList();
            DropPodUtility.DropThingsNear(parent.pawn.Position, parent.pawn.Map, pawnsToGenerate, faction: parent.pawn.Faction);
            LordMaker.MakeNewLord(parent.pawn.Faction, new LordJob_AssaultColony(), parent.pawn.Map, pawnsToGenerate);
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return true;
        }
        
        
        private IEnumerable<Pawn> CreateWave(Faction faction)
        {
            float combatPowerGeneratedSoFar = 0;
            
            while (combatPowerGeneratedSoFar < Props.combatScore)
            {
                PawnKindDef kind = Props.pawnKinds.RandomElement();
                combatPowerGeneratedSoFar += kind.combatPower;
                
                Pawn currentPawn = PawnGenerator.GeneratePawn(kind, faction);
                
                yield return currentPawn;
            }
        }

        public override bool CanCast => true;

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return true;
        }
    }
}