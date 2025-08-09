using RimWorld;
using Verse;

namespace PrimarchAssault
{
    [DefOf]
    public static class PADefsOf
    {
        public static HediffDef GWPA_Champion;
        public static HediffDef GWPA_ArmorFlinch;
        public static HediffDef GPWA_PsychicStormSuppression;
        public static DamageDef GWPA_ArmorTrauma;

        [MayRequire("HappyPurging.AgeofDarkness")]
        public static ThingDef GW_SM_DropPodIncomingImperial;
        //[MayRequire("HappyPurging.AgeofDarkness")]
        //public static ThingDef GW_SM_DropPodActiveImperial;
    }
}