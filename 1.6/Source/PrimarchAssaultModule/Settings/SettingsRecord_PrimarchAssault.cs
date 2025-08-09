using System.Collections.Generic;
using GW_Frame.Settings;
using RimWorld;
using Verse;

namespace PrimarchAssault.Settings
{
	public class SettingsRecord_PrimarchAssault: SettingsRecord
	{
		public List<string> DisabledEventActions => _disabledEventActions ??= new List<string>();
		
		private List<string> _disabledEventActions;

		public bool CanHealthbarBeMoved;
		
		
		public override void CastChanges() { }

		public override void Reset()
		{
			DisabledEventActions.Clear();
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref _disabledEventActions, "disabledEventActions");
			Scribe_Values.Look(ref CanHealthbarBeMoved, "canHealthBarBeMoved");
		}
	}
}