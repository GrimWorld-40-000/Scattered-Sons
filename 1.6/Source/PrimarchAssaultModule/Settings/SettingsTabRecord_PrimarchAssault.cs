using System;
using System.Collections.Generic;
using System.Linq;
using GW_Frame.Settings;
using PrimarchAssault.External;
using UnityEngine;
using Verse;

namespace PrimarchAssault.Settings
{
	public class SettingsTabRecord_PrimarchAssault(
		SettingsTabDef def,
		string label,
		Action clickedAction,
		Func<bool> selected)
		: SettingsTabRecord(def, label, clickedAction, selected)
	{
		private static SettingsRecord_PrimarchAssault _settingsRecord;
		public static SettingsRecord_PrimarchAssault SettingsRecord => _settingsRecord ??= GetRecord();

		private static SettingsRecord_PrimarchAssault GetRecord()
		{
			if (GW_Frame.Settings.Settings.Instance.TryGetModSettings(out SettingsRecord_PrimarchAssault record))
			{
				return record;
			}
			throw new Exception("Could not create or locate settings for Primarch Assault");
		}

		public override void OnGUI(Rect rect)
		{
			base.OnGUI(rect);
			
			
			Listing_Standard listingStandard = new Listing_Standard();
			listingStandard.Begin(rect.ContractedBy(20f));

			bool oldCanBeMoved = SettingsRecord.CanHealthbarBeMoved;
			
			listingStandard.CheckboxLabeled("GWPA.HealthbarMoveable".Translate(), ref SettingsRecord.CanHealthbarBeMoved);

			if (oldCanBeMoved != SettingsRecord.CanHealthbarBeMoved)
			{
				if (GameComponent_ChallengeManager.Instance != null)
				{
					GameComponent_ChallengeManager.Instance.HealthBar.draggable = SettingsRecord.CanHealthbarBeMoved;
					GameComponent_ChallengeManager.Instance.HealthBar.resizeable = SettingsRecord.CanHealthbarBeMoved;
				}
			}
			
			
			List<string> uniqueEventActionNames = new List<string>();
			
			foreach (AssaultEventDef assaultEventDef in DefDatabase<AssaultEventDef>.AllDefs)
			{
				foreach (var assaultEventActionProperties in assaultEventDef.actionProperties.Where(assaultEventActionProperties => !uniqueEventActionNames.Contains(assaultEventActionProperties.actionName)))
				{
					uniqueEventActionNames.Add(assaultEventActionProperties.actionName);
				}
			}
			
			foreach (string uniqueEventActionName in uniqueEventActionNames)
			{
				bool disabled = SettingsRecord.DisabledEventActions.Contains(uniqueEventActionName);
				
				GUI.color = disabled? Color.red : Color.white;
				
				listingStandard.Label(uniqueEventActionName);

				if (!listingStandard.ButtonText(disabled ? "Disabled" : "Enabled")) continue;
				if (disabled)
				{
					SettingsRecord.DisabledEventActions.Remove(uniqueEventActionName);
				}
				else
				{
					SettingsRecord.DisabledEventActions.Add(uniqueEventActionName);
				}
			}
			
			GUI.color = Color.white;
			
			listingStandard.End();
		}
	}
}