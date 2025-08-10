using HarmonyLib;
using PrimarchAssault.External;
using RimWorld;
using Verse;
namespace PrimarchAssault.HarmonyPatches
{
	[StaticConstructorOnStartup]
	public static class ApplyHarmonyPatches
	{
		static ApplyHarmonyPatches()
		{
			Harmony harmony = new Harmony("ThatHitmann.PrimarchAssault");
			harmony.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.PreApplyDamage)),
				new HarmonyMethod(typeof(ApplyHarmonyPatches), nameof(PreApplyDamage)));
		}
		public static bool PreApplyDamage(ref Pawn __instance, ref DamageInfo dinfo, ref bool absorbed)
		{
			//Does not take damage if they are a champion, instead marks it on their health bar
			if (__instance.health.hediffSet.TryGetHediff(PADefsOf.GWPA_Champion, out Hediff hediff) && hediff is Hediff_Champion champion)
			{
				//If they have a shield
				if (__instance.TryGetComp(out CompShield shield))
				{
				
					//Which is active
					if (shield.ShieldState == ShieldState.Active)
					{
						//Allow damage through, only if it's ranged or explosive
						if (dinfo.Def.isRanged || dinfo.Def.isExplosive)
						{
							return true;
						}
					}



				}
				float hpToRemove = HealthBarChangeAmount(dinfo);
				dinfo.SetAmount(0);
				absorbed = true;
			
				champion.DamageHealthBar(hpToRemove, dinfo);
			}
			return true;
		}
		//This calculates how much to actually affect the health bar based on the damage dealt. As-is this just uses exactly the amount of damage dealt
		private static float HealthBarChangeAmount(DamageInfo damageInfo)
		{
			return damageInfo.Amount;
		}
	}
}