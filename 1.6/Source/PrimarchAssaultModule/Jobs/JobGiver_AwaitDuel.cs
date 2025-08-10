using RimWorld;
using RimWorld.QuestGen;
using Verse;
using Verse.AI;

namespace PrimarchAssault.Jobs
{
	public class JobGiver_AwaitDuel : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn) => JobMaker.MakeJob(PADefsOf.GWPA_AwaitDuel);
	}
}