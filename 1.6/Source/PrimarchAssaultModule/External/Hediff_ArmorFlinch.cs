using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using PrimarchAssault;
using Verse;
using Verse.AI;

namespace PrimarchAssault.External
{
    public class Hediff_ArmorFlinch: Hediff
    {
        /*private const float ArmorReductionPerHit = 0.1f;
        
        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            
            StatModifier modifier = CurStage.statOffsets.First(modifier => modifier.stat == dinfo.Def.armorCategory.armorRatingStat);

            if (totalDamageDealt < 0.001)
            {
                modifier.value -= ArmorReductionPerHit * Severity;
            }
            else
            {
                modifier.value = 0;
            }
        }*/
        
        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);

            //Take BABT
            if (dinfo.IgnoreArmor || !(totalDamageDealt < 0.001f)) return;
            DamageInfo bruise = new DamageInfo(dinfo)
            {
                Def = PADefsOf.GWPA_ArmorTrauma
            };

            bruise.SetAmount(bruise.Amount * Severity);
            bruise.SetIgnoreArmor(true);
                
            pawn.TakeDamage(bruise);
        }
    }
}