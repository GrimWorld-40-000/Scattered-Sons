using Verse.AI;

namespace PrimarchAssault.External
{
	public class JobGiver_WanderNearDutyLocationIgnoreEnemies: JobGiver_WanderNearDutyLocation
	{
		public JobGiver_WanderNearDutyLocationIgnoreEnemies() : base()
		{
			this.expireOnNearbyEnemy = false;
		}
	}
}