using RimWorld;
using Verse;

namespace PrimarchAssault
{
    public class Alert_ChallengeIncoming: Alert_Critical
    {
        public override AlertReport GetReport()
        {
            //return AlertReport.Active;
            return GameComponent_ChallengeManager.Instance.IsPhaseOneQueued ? AlertReport.Active: AlertReport.Inactive;
        }

        public override string GetLabel()
        {
            return "GWPA.Incoming".Translate(GameComponent_ChallengeManager.Instance.QueuedPhaseOne?.LabelCap ?? "No event is queued. You shouldn't see this.");
        }

        public override TaggedString GetExplanation()
        {
            return "GWPA.IncomingDescription".Translate(GameComponent_ChallengeManager.Instance.QueuedPhaseOne?.championName ?? "Nobody is coming. You shouldn't see this.");
        }
    }
}