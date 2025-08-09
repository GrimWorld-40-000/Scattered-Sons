using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using PrimarchAssault.External;
using Verse;

namespace PrimarchAssault
{
    public static class DebugActions
    {

        [DebugAction("Primarch Assault", "Hasten revenge assaults", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void StartPhaseTwos()
        {
            GameComponent_ChallengeManager.Instance.StartAllPhaseTwos();
        }

        [DebugAction("Primarch Assault", "Fire assault event", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void FireAssaultEvent()
        {
            List<DebugMenuOption> list = DefDatabase<AssaultEventDef>.AllDefs.Select(assaultEventDef => new DebugMenuOption(assaultEventDef.label, DebugMenuOptionMode.Action, delegate { assaultEventDef.FireAllComps(Find.CurrentMap); })).ToList();

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
        }
        
    }
}