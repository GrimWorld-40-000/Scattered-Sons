using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PrimarchAssault.External;
using PrimarchAssault.Settings;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PrimarchAssault.AssaultEvent
{
    public abstract class AssaultEventAction
    {
        public AssaultEventDef parent;
        public AssaultEventActionProperties props;

        protected virtual IEnumerable<TargetInfo> GetTargets()
        {
            yield break;
        }
        
        public bool Enabled => !SettingsTabRecord_PrimarchAssault.SettingsRecord.DisabledEventActions.Contains(props.actionName);

        public virtual void Initialize(AssaultEventActionProperties loadingProps)
        {
            props = loadingProps;
        }
        
        protected bool TryGetSpawnedChampion(out Pawn champion)
        {
            List<Pawn> pawns = Find.AnyPlayerHomeMap.mapPawns.AllHumanlikeSpawned.Where(pawn =>
                pawn.health.hediffSet.HasHediff(PADefsOf.GWPA_Champion) && pawn.SpawnedOrAnyParentSpawned).ToList();
            champion = pawns.EnumerableNullOrEmpty() ? null : pawns.First();
            return champion != null;
        }

        public virtual void Apply(Map map)
        {
            Messages.Message(props.eventNotificationText, new LookTargets(GetTargets()), MessageTypeDefOf.NegativeEvent);

            if (TryGetSpawnedChampion(out Pawn champion))
            {
                if (props.fleckOnChampion != null)
                {
                    FleckMaker.AttachedOverlay(champion, props.fleckOnChampion, Vector3.zero);
                }
                props.sound?.PlayOneShot(new TargetInfo(champion.Position, champion.Map));
            }
            else
            {
                props.sound?.PlayOneShotOnCamera();
            }
        }
    }
}