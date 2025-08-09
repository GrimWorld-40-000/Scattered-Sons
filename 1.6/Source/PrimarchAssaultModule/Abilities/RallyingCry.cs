using System.Linq;
using RimWorld;
using Verse;

namespace PrimarchAssault.Abilities
{
    public class HediffCompProperties_RallyingCry : HediffCompProperties
    {

        public HediffDef rallyHediff;
        public int rallyRange;


        public HediffCompProperties_RallyingCry()
        {
            compClass = typeof(HediffComp_RallyingCry);
        }
    }
    
    public class HediffComp_RallyingCry: HediffComp
    {
        public HediffCompProperties_RallyingCry Props => (HediffCompProperties_RallyingCry) props;
        
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            if (ModLister.AnomalyInstalled)
            {
                EffecterDefOf.ChimeraRage.Spawn(parent.pawn.Position, parent.pawn.Map).Cleanup();
            }
            TriggerRally();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (!parent.pawn.IsHashIntervalTick(2500)) return;
            TriggerRally();
        }

        private void TriggerRally()
        {
            if (!parent.pawn.Spawned) return;
            foreach (Pawn pawn1 in parent.pawn.Map.mapPawns.AllPawns.Where(pawn => !pawn.HostileTo(parent.pawn) && pawn.Position.DistanceTo(parent.pawn.Position) <= Props.rallyRange && pawn != parent.pawn))
            {
                pawn1.health.AddHediff(Props.rallyHediff);
            }
        }
    }
}