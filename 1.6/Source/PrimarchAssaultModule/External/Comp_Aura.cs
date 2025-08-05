using RimWorld;
using UnityEngine;
using Verse;

namespace PrimarchAssault.External
{
	public class CompProperties_Aura : CompProperties
	{
		public float radius;
		
		public CompProperties_Aura()
		{
			compClass = typeof(Comp_Aura);
		}
	}
	
	[StaticConstructorOnStartup]
	public class Comp_Aura: ThingComp
	{
		public CompProperties_Aura Props => (CompProperties_Aura)props;
		
		//private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);
		private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/Aura", ShaderDatabase.Transparent);
		
		protected Pawn PawnOwner => parent as Pawn;

		public override void PostDraw()
		{
			base.PostDraw();
			
			/*Vector3 drawPos = PawnOwner.Drawer.DrawPos with
			{
				y = AltitudeLayer.MoteOverhead.AltitudeFor()
			};
			float angle = Rand.Range(0, 360);
			Vector3 s = new Vector3(Props.radius, 1f, Props.radius);
			Matrix4x4 matrix = new Matrix4x4();
			matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);*/
			
			
			float num1 = Props.radius;
			Vector3 drawPos = PawnOwner.Drawer.DrawPos with
			{
				y = AltitudeLayer.MoteOverhead.AltitudeFor()
			};
			Vector3 s = new Vector3(num1, 1f, num1);
			Matrix4x4 matrix = new Matrix4x4();
			matrix.SetTRS(drawPos, Quaternion.AngleAxis(0, Vector3.up), s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
		}
	}
}