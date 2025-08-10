using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace PrimarchAssault.Jobs
{
	public abstract class LordToil_DuelToil: LordToil
	{
		
		protected LordToilData_GuardDuel Data => (LordToilData_GuardDuel)data;

		
		public LordToil_DuelToil(IEnumerable<Pawn> participants, Pawn duelist)
		{
			data = new LordToilData_GuardDuel();
			Data.guards = participants.ToList();
			Data.duelist = duelist;
		}

		public override void UpdateAllDuties()
		{
			throw new System.NotImplementedException();
		}

		

		private static int SquaredEnemyRange = 144;
		protected bool AnyEnemyNear(Pawn pawn)
		{
			if (pawn.Map == null)
			{
				return false;
			}
			
			foreach (Pawn pawn1 in pawn.Map.mapPawns.AllHumanlikeSpawned)
			{
				if (pawn1 != null)
				{
					if (pawn.HostileTo(pawn1))
					{
						int distance = pawn.PositionHeld.DistanceToSquared(pawn1.PositionHeld);
						if (distance < SquaredEnemyRange)
						{
							return true;
						}
					}
				}
			}

			return false;
		}
	}
}