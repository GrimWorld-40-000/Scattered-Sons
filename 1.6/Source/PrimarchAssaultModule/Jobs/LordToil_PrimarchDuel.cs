using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine.Assertions;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace PrimarchAssault.Jobs
{
	public class LordToil_PrimarchDuel : LordToil_DuelToil
	{

		public LordToil_PrimarchDuel(IEnumerable<Pawn> participants, Pawn duelist) : base(participants, duelist)
		{
		}
		

		public override void UpdateAllDuties()
		{
			LordJob_Duel job = (LordJob_Duel)lord.LordJob;
			if (Data.duelist == null)
			{
				Log.Error("Tried to guard duel with null duelist.");
				return;
			}

			if (job == null)
			{
				Log.Error("Tried to attach Primarch Duel Toil to non-duel Lord Job.");
				return;
			}
			foreach (Pawn pawn in Data.guards)
			{
				if (pawn?.mindState != null)
					pawn.mindState.duty = new PawnDuty(PADefsOf.GWPA_GuardDuelDuty, Data.duelist);
					
					
			}
			
			job.FindDuelist();
			
			if (Data.duelist.mindState != null)
				Data.duelist.mindState.duty = new PawnDuty(PADefsOf.GWPA_PrimarchDuel, Data.duelist.PositionHeld);
			
			if (job.GetDuelistPawn().mindState != null)
				job.GetDuelistPawn().mindState.duty = new PawnDuty(PADefsOf.GWPA_PrimarchDuel, Data.duelist.PositionHeld);
		}
		
	}
}