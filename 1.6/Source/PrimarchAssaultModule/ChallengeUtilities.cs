using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PrimarchAssault
{
    public static class ChallengeUtilities
    {
	    

		public static ThingDef RandomNotOwnedThingFromList(List<ThingDef> listOfThings, Map map)
		{
			if (!listOfThings.Any())
			{
				Log.Error("There were no items in duel champion's pool.");
				return null;
			}
			
			
			List<ThingDef> thingCandidates = listOfThings.ToList();
			
			foreach (ThingDef thing in listOfThings)
			{
				int count = map.listerThings.AllThings.Count(thing1 => thing1.def == thing) + GetCarriedCount(thing, map);
				if (thing.IsApparel)
				{
					foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
					{
						List<Apparel> wornApparel = pawn.apparel.WornApparel;
						for (int index = 0; index < wornApparel.Count; ++index)
						{
							if (CountValidThing(wornApparel[index], thing))
								count += wornApparel[index].stackCount;
						}
						//Count apparel also
					}
				}

				if (count > 0)
				{
					Log.Message("Removing a " + thing.label);
					thingCandidates.Remove(thing);
				}
			}

			return thingCandidates.Any() ? thingCandidates.RandomElement() : listOfThings.RandomElement();
		}
		
		
		private static int GetCarriedCount(ThingDef prodDef, Map map)
		{
			int carriedCount = 0;
			foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
			{
				Thing carriedThing = pawn.carryTracker.CarriedThing;
				if (carriedThing != null)
				{
					int stackCount = carriedThing.stackCount;
					if (CountValidThing(carriedThing.GetInnerIfMinified(), prodDef))
						carriedCount += stackCount;
				}
			}
			return carriedCount;
		}
		
		public static bool CountValidThing(Thing thing, ThingDef def)
		{
			ThingDef def1 = thing.def;
			if (def1 != def)
				return false;
			return !thing.PositionHeld.Fogged(thing.MapHeld);
		}
    }
}