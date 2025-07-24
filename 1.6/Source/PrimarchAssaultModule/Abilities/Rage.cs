using RimWorld;
using Verse;

namespace PrimarchAssault.Abilities
{
    public class HediffCompProperties_Rage : HediffCompProperties
    {
        public HediffCompProperties_Rage()
        {
            compClass = typeof(HediffComp_Rage);
        }
    }
    
    public class HediffComp_Rage: HediffComp
    {
        public HediffCompProperties_Rage Props => (HediffCompProperties_Rage) props;
        
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            if (ModLister.AnomalyInstalled)
            {
                EffecterDefOf.ChimeraRage.Spawn(parent.pawn.Position, parent.pawn.Map).Cleanup();
            }
        }
    }
}