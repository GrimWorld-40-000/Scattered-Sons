using System;
using System.Collections.Generic;
using Verse;

namespace PrimarchAssault.External
{
	public class WaveBlueprint
	{
		public bool spawnsChampion = false;
		public List<WaveDetails> possibleWavesInPriorityOrder;
		public Type lordJobOverrideType = null;
	}
}