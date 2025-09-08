using RimWorld;
using Verse;
using Verse.AI;

namespace PrimarchAssault
{
    [DefOf]
    public static class PADefsOf
    {
        public static HediffDef GWPA_Champion;
        public static HediffDef GWPA_Duelist;
        public static HediffDef GWPA_DuelVictor;
        public static HediffDef GWPA_ArmorFlinch;
        public static HediffDef GPWA_PsychicStormSuppression;
        public static DamageDef GWPA_ArmorTrauma;
        public static DutyDef GWPA_WanderCloseIgnoreEnemies;
        public static JobDef GWPA_GuardDuel;
        public static JobDef GWPA_AwaitDuel;
        public static DutyDef GWPA_GuardDuelDuty;
        public static DutyDef GWPA_PrepareToDuelDuty;
        public static DutyDef GWPA_PrimarchDuel;
        public static MentalStateDef GWPA_Dueling;
        public static ConceptDef GWPA_LionDuel;
        
        [MayRequire("HappyPurging.AgeofDarkness")]
        public static ThingDef GW_SM_DropPodIncomingImperial;
        //[MayRequire("HappyPurging.AgeofDarkness")]
        //public static ThingDef GW_SM_DropPodActiveImperial;
    }
}