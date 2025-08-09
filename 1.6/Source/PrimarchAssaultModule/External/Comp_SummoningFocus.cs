using System.Collections.Generic;
using RimWorld;
using PrimarchAssault;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PrimarchAssault.External
{
    public class CompProperties_SummoningFocus: CompProperties
    {
        public List<ChallengeDef> summons;

        public CompProperties_SummoningFocus()
        {
            compClass = typeof(Comp_SummoningFocus);
        }
    }
    
    public class Comp_SummoningFocus: ThingComp
    {
        
        public CompProperties_SummoningFocus Props => (CompProperties_SummoningFocus)props;

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption compFloatMenuOption in base.CompFloatMenuOptions(selPawn))
            {
                yield return compFloatMenuOption;
            }
            foreach (ChallengeDef challengeDef in Props.summons)
            {
                yield return new FloatMenuOption(challengeDef.LabelCap, () =>
                {
                    DoChallenge(challengeDef);
                }, challengeDef.Icon, Color.white, challengeDef.CanDoNow()? MenuOptionPriority.Default: MenuOptionPriority.DisabledOption)
                {
                    Disabled = !challengeDef.CanDoNow()
                };
            }
        }


        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            foreach (ChallengeDef challengeDef in Props.summons)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        DoChallenge(challengeDef);
                    },
                    icon = challengeDef.Icon,
                    defaultLabel = challengeDef.LabelCap,
                    Disabled = !challengeDef.CanDoNow(out string reason),
                    defaultDesc = challengeDef.description,
                    disabledReason = reason
                };
            }
        }


        private void DoChallenge(ChallengeDef challengeDef)
        {
            
            if (challengeDef.effectorDef != null)
            {
                Effecter effector = new Effecter(challengeDef.effectorDef);
                effector.Trigger(new TargetInfo(parent.Position, parent.Map), TargetInfo.Invalid);
                effector.Cleanup();
            }
            
            GameComponent_ChallengeManager.Instance.StartPhaseOne(challengeDef);
        }
    }
}