using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PrimarchAssault.AssaultEvent
{
	public class TeleportIntoMountainEventProperties : AssaultEventActionProperties
	{
		public float scatterRadius;
		
		public override Type AssaultEventClass() => typeof(TeleportIntoMountainEvent);
	}

	public class TeleportIntoMountainEvent: AssaultEventAction
	{

		private IntVec3 mountainCell = IntVec3.Invalid;
		private Map teleportMap;
		
		private TeleportIntoMountainEventProperties Props => (TeleportIntoMountainEventProperties) props;
		
		protected override IEnumerable<TargetInfo> GetTargets()
		{
			yield return new TargetInfo(mountainCell, teleportMap);
		}
		
		
		
		
		
		public override void Apply(Map map)
		{
			int attempts = 0;

			while (attempts < 10)
			{
				if (!InfestationCellFinder.TryFindCell(out mountainCell, map))
				{
					attempts++;
					continue;
				}
				if (map.areaManager.Home.ActiveCells.Contains(mountainCell)) break;
				attempts++;
			}

			if (attempts >= 10)
			{
				return;
			}			
			
			
			if (!TryGetSpawnedChampion(out Pawn champion)) return;
			
			
			base.Apply(map);
			teleportMap = map;
				
			
			Faction faction = champion.Faction;
			
				
			foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned.Where(pawn => !pawn.HostileTo(faction)).ToList())
			{
				if (!FindFreeCellNear(mountainCell, map, out var result)) continue;
				AbilityUtility.DoClamor(pawn.Position, Props.scatterRadius, pawn, ClamorDefOf.Ability);
				AbilityUtility.DoClamor(result, Props.scatterRadius, pawn, ClamorDefOf.Ability);
					
				EffecterDefOf.Skip_Entry.Spawn(pawn.Position, map, 0.72f).Cleanup();
				EffecterDefOf.Skip_ExitNoDelay.Spawn(result, map, 0.72f).Cleanup();

				FleckMaker.ThrowDustPuffThick(result.ToVector3(), map, Rand.Range(1.5f, 3f), CompAbilityEffect_Chunkskip.DustColor);
				SkipUtility.SkipTo(pawn, result, map);
			}
				
				
				
			SoundDefOf.Psycast_Skip_Pulse.PlayOneShot(new TargetInfo(mountainCell, map));
		}
	
		private bool FindFreeCellNear(IntVec3 target, Map map, out IntVec3 result)
		{
			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (CellFinder.TryFindRandomCellNear(target, map, Mathf.RoundToInt(Props.scatterRadius) - 1,
				    cell => CompAbilityEffect_WithDest.CanTeleportThingTo(cell, map) &&
				            GenSight.LineOfSight(cell, target, map, true), out result))
			{
				return true;
			}

			return FindFreeCellNearFallback(target, map, out result);
		}
		
		private bool FindFreeCellNearFallback(IntVec3 target, Map map, out IntVec3 result)
		{
			return CellFinder.TryFindRandomCellNear(target, map, Mathf.RoundToInt(Props.scatterRadius) * 2 - 1, cell => CompAbilityEffect_WithDest.CanTeleportThingTo(cell, map), out result);
		}
	}
}