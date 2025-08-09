using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using PrimarchAssault;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace PrimarchAssault.External
{
    public class ChallengeDef: Def
    {
        
        public string iconPath;
        public string healthBarPath;

        public string arrivalText;

        public string championName;

        public List<SongDef> playlist;

        public ChampionDetails championDetails;

        public List<WaveDetails> phaseOneWavesInPriority;
        public List<WaveDetails> phaseTwoWavesInPriority;
        public List<WaveDetails> escortWavesInPriority;

        public List<ChampionAbilityStage> abilityStages;
        public List<ChampionHediffStage> hediffStages;
        public List<ChampionEventStage> eventStages;
        public ThingDef championDrop;

        private Color primarchColor;

        public int healthBarX;
        public int healthBarY;
        public int healthBarWidth;
        public int healthBarHeight;
        public int shieldBarX;
        public int shieldBarY;
        public int shieldBarWidth;
        public int shieldBarHeight;
        
        public Rect healthBarRelative => _healthBarRelative ??= new Rect(healthBarX, healthBarY, healthBarWidth, healthBarHeight);
        private Rect? _healthBarRelative;
        public Rect shieldBarRelative => _shieldBarRelative ??= new Rect(shieldBarX, shieldBarY, shieldBarWidth, shieldBarHeight);
        private Rect? _shieldBarRelative;
        
        public SoundDef announcementSound;
        
        public EffecterDef effectorDef;
        //1 to 4 in-game hours
        public IntRange ticksUntilArrival = new IntRange(2500, 10000);
        //2 quadrums to 1 year
        public IntRange ticksUntilRevenge = new IntRange(1800000, 3600000);

        public int ticksUntilChampionArrives = 1500;

        public float championFlinchSeverity;
        
        public List<ResearchProjectDef> researchRequirements = new List<ResearchProjectDef>();
        
        public Texture2D Icon { get; private set; }
        public Texture2D HealthBarIcon { get; private set; }

        private bool IsResearchDone()
        {
            return researchRequirements.All(researchProjectDef => researchProjectDef.IsFinished);
        }

        private string IncompleteResearch()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var researchProjectDef in researchRequirements.Where(researchProjectDef => !researchProjectDef.IsFinished))
            {
                builder.AppendLine().Append(researchProjectDef.LabelCap);
            }
            return builder.ToString();
        }

        public bool CanDoNow()
        {
            return CanDoNow(out _);
        }

        public bool CanDoNow(out string disabledReason)
        {
            if (!IsResearchDone())
            {
                disabledReason = "GWPA.ResearchIncomplete".Translate(IncompleteResearch());
                return false;
            }

            if (GameComponent_ChallengeManager.Instance.CanStartNewChallenge(this))
            {
                disabledReason = "";
                return true;
            }

            disabledReason = "GWPA.AlreadyQueued".Translate();
            return false;

        }
        
        public override void PostLoad()
        {
            base.PostLoad();
            if (iconPath == null)
            {
                Icon = BaseContent.BadTex;
            }
            else
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    try
                    {
                        Icon = ContentFinder<Texture2D>.Get(iconPath);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Icon = BaseContent.BadTex;
                        throw;
                    }
                });
            }
            if (healthBarPath == null)
            {
                HealthBarIcon = null;
            }
            else
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    try
                    {
                        HealthBarIcon = ContentFinder<Texture2D>.Get(healthBarPath);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        HealthBarIcon = BaseContent.BadTex;
                        throw;
                    }
                });
            }
        }

        public void FirePhaseOne()
        {
            SpawnAssault(false, phaseOneWavesInPriority.First());
        }
        
        public void FirePhaseTwo()
        {
            SpawnAssault(true, phaseTwoWavesInPriority.First());
        }

        private IEnumerable<Pawn> CreateWave(WaveDetails details, Map map, Faction faction)
        {
            float combatPowerGoal = Math.Min(map.PlayerWealthForStoryteller, details.wealthCap) * details.wealthMultiplier;
            float combatPowerGeneratedSoFar = 0;

            while (combatPowerGeneratedSoFar < combatPowerGoal)
            {
                PawnKindDef kind = details.pawnKinds.RandomElement();
                combatPowerGeneratedSoFar += kind.combatPower;


                Pawn currentPawn = PawnGenerator.GeneratePawn(kind, faction);
                if (details.flinchSeverity > 0)
                {
                    Hediff armorFlinch = HediffMaker.MakeHediff(PADefsOf.GWPA_ArmorFlinch, currentPawn);
                    armorFlinch.Severity = details.flinchSeverity;
                    currentPawn.health.AddHediff(armorFlinch);
                }
                
                
                
                yield return currentPawn;
            }
        }

        public void SpawnChampion(bool isPhaseTwo, ThingDef championsChampionDrop)
        {
            announcementSound.PlayOneShotOnCamera();
            
            Map map = Find.AnyPlayerHomeMap;
            Faction faction = championDetails.GetFirstValidFactionOrNull();
            
            //Create Champion
            Pawn champion = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                championDetails.possiblePawnKinds.RandomElement(), mustBeCapableOfViolence: true,
                biocodeWeaponChance: 1, biocodeApparelChance: 1, forcedTraits: championDetails.forcedTraits,
                forcedXenotype: championDetails.GetFirstValidXenotypeOrNull(),
                faction: faction,
                fixedBiologicalAge: championDetails.forcedBiologicalAge,
                fixedChronologicalAge: championDetails.forcedChronologicalAge,
                fixedGender: championDetails.forcedGender, fixedLastName: championDetails.forcedLastname,
                fixedBirthName: championDetails.forcedName, maximumAgeTraits: 0));
            champion.Name = new NameTriple(championDetails.forcedName, championDetails.forcedNickname,
                championDetails.forcedLastname);

            //Create the champion's hediff. This will give special abilities and make them drop the item on death.
            Hediff_Champion championHediff = (Hediff_Champion) HediffMaker.MakeHediff(PADefsOf.GWPA_Champion, champion);

            List<ChampionStage> stages = new List<ChampionStage>();
            if (abilityStages != null)
            {
                stages.AddRange(abilityStages);
            }
            if (hediffStages != null)
            {
                stages.AddRange(hediffStages);
            }
            if (eventStages != null)
            {
                stages.AddRange(eventStages);
            }
            
            championHediff.SetupHediff(championsChampionDrop, stages, this, isPhaseTwo);
            champion.health.AddHediff(championHediff);

            if (championFlinchSeverity > 0)
            {
                Hediff armorFlinch = HediffMaker.MakeHediff(PADefsOf.GWPA_ArmorFlinch, champion);
                armorFlinch.Severity = championFlinchSeverity;
                champion.health.AddHediff(armorFlinch);
            }


            List<Pawn> allOfFaction = map.mapPawns.AllHumanlikeSpawned.Where(pawn => pawn.Faction == faction).ToList();

            IntVec3 epicenter;
            
            if (allOfFaction.NullOrEmpty())
            {
                if (!CellFinder.TryFindRandomCellNear(map.Center, map, 5,
                        vec3 => map.reachability.CanReachColony(vec3), out epicenter))
                {
                    return;
                }
            }
            else
            {
                int count = allOfFaction.Count;
                float x = 0;
                float y = 0;
                float z = 0;
                foreach (Pawn pawn in allOfFaction)
                {
                    x += (float)pawn.Position.x / count;
                    y += (float)pawn.Position.y / count;
                    z += (float)pawn.Position.z / count;
                }
                epicenter = new IntVec3((int)x, (int)y, (int)z);
            }
            
            foreach (Pawn pawn in allOfFaction)
            {
                if (pawn.TryGetLord(out Lord lord))
                {
                    lord.RemoveAllPawns();
                }
            }
            
            List<Pawn> pawnsToGenerate = CreateWave(escortWavesInPriority.First(), map, faction).ToList();
            pawnsToGenerate.Add(champion);
            allOfFaction.AddRange(pawnsToGenerate);

            //ColorDropPodsNext(primarchColor);
            DropPodUtility.DropThingsNear(epicenter, map, pawnsToGenerate, faction: faction);
            //ColorDropPodsOnMap(map, primarchColor);
            
            LordMaker.MakeNewLord(faction, new LordJob_AssaultColony(), map, allOfFaction);
            
            GameComponent_ChallengeManager.Instance.RegisterActiveChampion(champion.thingIDNumber, this);
            
            
            eventStages?.DoIf(stage => stage.triggerOnChampionArrive, stage => stage.Apply(null, map));
        }

        private void SpawnAssault(bool useCenterByDefault, WaveDetails details)
        {
            Map map = Find.AnyPlayerHomeMap;
            Find.TickManager.slower.SignalForceNormalSpeedShort();
            Faction faction = championDetails.GetFirstValidFactionOrNull();
            
              
            Find.MusicManagerPlay.ForcePlaySong(playlist.RandomElement(), false);
            
            
            if (!faction.HostileTo(Faction.OfPlayer))
            {
                faction.TryAffectGoodwillWith(Faction.OfPlayer, -200);
            }

            bool mustUseDropPods = false;
            if (useCenterByDefault || !CellFinder.TryFindRandomEdgeCellWith(vec3 => map.reachability.CanReachColony(vec3), map,
                    0.8f, out var spawnCell))
            {
                if (!CellFinder.TryFindRandomCellNear(map.Center, map, 5,
                        vec3 => map.reachability.CanReachColony(vec3), out spawnCell))
                {
                    Log.Error("Couldn't find any valid spawn location for assault");
                    return;
                }
                mustUseDropPods = true;
            }

            
            //Create wave
            List<Pawn> pawnsToGenerate = CreateWave(details, map, faction).ToList();

            
            
            if (mustUseDropPods)
            {
                //ColorDropPodsNext(primarchColor);
                DropPodUtility.DropThingsNear(spawnCell, map, pawnsToGenerate, faction: faction);
                //ColorDropPodsOnMap(map, primarchColor);
            }
            else
            {
                foreach (Pawn pawn in pawnsToGenerate)
                {
                    if (!CellFinder.TryFindRandomCellNear(spawnCell, map, 5,
                            vec3 => map.reachability.CanReachColony(vec3), out IntVec3 spawnPointForIndividual))
                    {
                        spawnPointForIndividual = spawnCell;
                    }

                    GenSpawn.Spawn(pawn, spawnPointForIndividual, map);
                }
            }

            LordMaker.MakeNewLord(faction, new LordJob_AssaultColony(), map, pawnsToGenerate);
            
            Find.LetterStack.ReceiveLetter(LabelCap, arrivalText, LetterDefOf.ThreatBig, new LookTargets(pawnsToGenerate));

            eventStages?.DoIf(stage => stage.triggerOnAssaultStart, stage => stage.Apply(null, map));
        }

        private static void ColorDropPodsOnMap(Map map, Color color)
        {
            if (!ModLister.AnyFromListActive(new List<string> { "HappyPurging.AgeofDarkness" })) return;
            foreach (Thing thing in map.listerThings.ThingsOfDef(PADefsOf.GW_SM_DropPodIncomingImperial))
            {
                thing.Graphic.color = color;

                //thing.Graphic

                //thing.DrawColor = color;

                //thing.Graphic.graphicInt = this.def.graphicData.GraphicColoredFor(this);

                //Log.Message("1");
            }
        }
        
        

        private static void ColorDropPodsNext(Color color)
        {
            if (!ModLister.AnyFromListActive(new List<string> { "HappyPurging.AgeofDarkness" })) return;
            Traverse.Create(PADefsOf.GW_SM_DropPodIncomingImperial.graphicData).Field("cachedGraphic").SetValue(null);
            PADefsOf.GW_SM_DropPodIncomingImperial.graphicData.Graphic.color = color;
        }
    }
}