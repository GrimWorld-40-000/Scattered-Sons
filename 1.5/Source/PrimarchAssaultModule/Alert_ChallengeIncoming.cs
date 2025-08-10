using RimWorld;
using Verse;

namespace PrimarchAssault
{
    public class Alert_ChallengeIncoming: Alert_Critical
    {
        public override AlertReport GetReport()
        {
            //return AlertReport.Active;
            //TODO Phase one is removed
            //return GameComponent_ChallengeManager.Instance.IsPhaseOneQueued ? AlertReport.Active: AlertReport.Inactive;
            return GameComponent_ChallengeManager.Instance.IsAnyAssaultQueued ? AlertReport.Active: AlertReport.Inactive;
        }

        public override string GetLabel()
        {
	        //TODO Phase one is removed
            // return "GWPA.Incoming".Translate(GameComponent_ChallengeManager.Instance.QueuedPhaseOne?.LabelCap ?? "No event is queued. You shouldn't see this.");
            return "GWPA.Incoming".Translate(GameComponent_ChallengeManager.Instance.FirstQueuedAssault?.LabelCap ?? "No event is queued. You shouldn't see this.");
        }

        public override TaggedString GetExplanation()
        {
	        //TODO Phase one is removed
            // return "GWPA.IncomingDescription".Translate(GameComponent_ChallengeManager.Instance.QueuedPhaseOne?.championName ?? "Nobody is coming. You shouldn't see this.");
            return "GWPA.IncomingDescription".Translate(GameComponent_ChallengeManager.Instance.FirstQueuedAssault?.championName ?? "Nobody is coming. You shouldn't see this.");
        }
    }
}