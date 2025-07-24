using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PrimarchAssault.GameConditions
{
	[StaticConstructorOnStartup]
	public class OrbitalBombardment : BombardmentParent
	{
		private List<Bombardment.BombardmentProjectile> _projectiles = new List<Bombardment.BombardmentProjectile>();
		private readonly List<Bombardment.BombardmentProjectile> _projectilesToRemove = new List<Bombardment.BombardmentProjectile>();

		private static readonly Material ProjectileMaterial =
			MaterialPool.MatFrom("Things/Projectile/Bullet_Big", ShaderDatabase.Transparent, Color.white);


		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode != LoadSaveMode.PostLoadInit) return;
			_projectiles ??= new List<Bombardment.BombardmentProjectile>();
		}

		public override void GameConditionTick()
		{
			base.GameConditionTick();
			
			_projectilesToRemove.Clear();
			
			foreach (Bombardment.BombardmentProjectile bombardmentProjectile in _projectiles)
			{
				bombardmentProjectile.Tick();
				Draw();
				if (bombardmentProjectile.LifeTime > 0) continue;
				IntVec3 targetCell = bombardmentProjectile.targetCell;
				GenExplosion.DoExplosion(targetCell, SingleMap, Rand.Range(2f, 5f), DamageDefOf.Bomb, null);
				_projectilesToRemove.Add(bombardmentProjectile);
			}
			
			foreach (Bombardment.BombardmentProjectile bombardmentProjectile in _projectilesToRemove)
			{
				_projectiles.Remove(bombardmentProjectile);
			}
		}

		protected override void Trigger()
		{
			base.Trigger();
			
			SoundDefOf.Bombardment_PreImpact.PlayOneShot(new TargetInfo(NextTargetCell, SingleMap));
			_projectiles.Add(new Bombardment.BombardmentProjectile(200, NextTargetCell));
		}

		private void Draw()
		{
			if (_projectiles.NullOrEmpty()) return;
			
			foreach (Bombardment.BombardmentProjectile bombardmentProjectile in _projectiles)
			{
				bombardmentProjectile.Draw(ProjectileMaterial);
			}
		}
	}
}