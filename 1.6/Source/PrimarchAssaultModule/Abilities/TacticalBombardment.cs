using System.Collections.Generic;
using System.Linq;
using RimWorld;
using PrimarchAssault;
using Verse;
using Verse.Sound;

namespace PrimarchAssault.Abilities
{
    public class CompProperties_TacticalBombardment: AbilityCompProperties
    {
        public int bombCount;
        public float maxRange;
        public int tickBeforeFirstLand;
        public float targetMortarChance;
        public float targetPawnChance;
        
        public CompProperties_TacticalBombardment()
        {
            compClass = typeof(CompAbilityEffect_TacticalBombardment);
        }
    }
    
    public class CompAbilityEffect_TacticalBombardment: CompAbilityEffect
    {
        
        private CompProperties_TacticalBombardment Props => (CompProperties_TacticalBombardment)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            List<Thing> thingsInRange = GenRadial.RadialDistinctThingsAround(target.Cell, parent.pawn.Map, Props.maxRange, true).ToList();
            List<IntVec3> pawnsInRange = thingsInRange.Where(thing => thing is Pawn && thing.HostileTo(parent.pawn))
                .Select(thing => thing.Position).ToList();
            List<IntVec3> mortarsInRange = thingsInRange.Where(thing => thing.def == ThingDefOf.Turret_Mortar && thing.HostileTo(parent.pawn))
                .Select(thing => thing.Position).ToList();

            int ticksUntilLand = Props.tickBeforeFirstLand;

            for (int i = 0; i < Props.bombCount; i++)
            {
                IntVec3 targetPoint = GetBombLocation(pawnsInRange, mortarsInRange, target.Cell);
                MoteMaker.MakeBombardmentMote(targetPoint, parent.pawn.Map, 1.5f);
                Bombardment bombardment = (Bombardment) GenSpawn.Spawn(ThingDefOf.Bombardment, targetPoint, parent.pawn.Map);
                bombardment.explosionCount = 3;
                bombardment.warmupTicks = ticksUntilLand;
                bombardment.instigator = parent.pawn;
                bombardment.explosionRadiusRange = new FloatRange(3, 7.5f);
                bombardment.StartStrike();
                ticksUntilLand += Rand.Range(500, 1500);
            }
        }

        private IntVec3 GetBombLocation(List<IntVec3> pawnCandidates, List<IntVec3> mortarCandidates, IntVec3 center)
        {
            IntVec3 target;
            if (Rand.Chance(Props.targetMortarChance) && !mortarCandidates.Empty())
            {
                int index = Rand.Range(0, mortarCandidates.Count);
                target = mortarCandidates[index];
                mortarCandidates.RemoveAt(index);
            }
            else if (Rand.Chance(Props.targetPawnChance) && !pawnCandidates.Empty())
            {
                int index = Rand.Range(0, pawnCandidates.Count);
                target = pawnCandidates[index];
                pawnCandidates.RemoveAt(index);
            }
            else
            {
                target = GenRadial.RadialCellsAround(center, Props.maxRange, true).Where(vec3 => vec3.InBounds(parent.pawn.Map)).RandomElement();
            }

            return target;
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return true;
        }

        public override bool CanCast => true;

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return true;
        }
    }
}