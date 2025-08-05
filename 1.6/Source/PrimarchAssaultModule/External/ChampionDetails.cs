using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PrimarchAssault.External
{
    public class ChampionDetails
    {
        public string forcedName;
        public string forcedLastname;
        public string forcedNickname;
        public int forcedChronologicalAge;
        public int forcedBiologicalAge;
        public List<TraitDef> forcedTraits;
        public Gender forcedGender;
        public List<PawnKindDef> possiblePawnKinds;
        public List<XenotypeDef> forcedXenotypesInOrder;
        public List<FactionDef> forcedSpawnFactionsInOrder;
        
        public XenotypeDef GetFirstValidXenotypeOrNull()
        {
            return forcedXenotypesInOrder.NullOrEmpty() ? null : forcedXenotypesInOrder.First();
        }
        public Faction GetFirstValidFactionOrNull()
        {
            if (forcedSpawnFactionsInOrder.NullOrEmpty()) return null;
            
            Faction factionCandidate = null;
            
            foreach (FactionDef factionDef in forcedSpawnFactionsInOrder)
            {
                factionCandidate = Find.FactionManager.FirstFactionOfDef(factionDef);
                if (factionCandidate != null) break;
            };

            return factionCandidate;
        }
    }
}