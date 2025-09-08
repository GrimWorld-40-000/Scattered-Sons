using System;
using System.Collections.Generic;

namespace PrimarchAssault.External
{
	public class WaveBlueprint
	{
		public bool repeat = false;
		public bool spawnsChampion = false;
		public List<WaveDetails> possibleWavesInPriorityOrder;
		public Type lordJobOverrideType = null;
	}
}