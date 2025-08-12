using System.Collections.Generic;
using PrimarchAssault;
using PrimarchAssault.Jobs;
using Verse;
using Verse.AI.Group;

namespace PrimarchAssault
{
	public class LordJob_AuraFarm: LordJob_Champion
	{
		public LordJob_AuraFarm(Pawn duelist, List<Pawn> guards) : base(duelist, guards)
		{
			point = duelist.PositionHeld;
		}

		public LordJob_AuraFarm() {}

		public IntVec3 point;
		
		public override StateGraph CreateGraph()
		{
			StateGraph graph = new StateGraph();
			

			LordToil_DefendPoint idle = new LordToil_DefendPoint(point, 20, 10);
			graph.AddToil(idle);

			
			return graph;
		}
		
		
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref point, "point");
		}
	}
}