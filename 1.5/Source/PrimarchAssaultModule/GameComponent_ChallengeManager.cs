using System.Collections.Generic;
using System.Linq;
using PrimarchAssault.External;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PrimarchAssault
{
    public class GameComponent_ChallengeManager : GameComponent
    {
        private static Game _game;

        public GameComponent_ChallengeManager(Game game)
        {
            _game = game;
        }

        public readonly HealthBarWindow HealthBar = new HealthBarWindow();

        public static GameComponent_ChallengeManager Instance => _game.GetComponent<GameComponent_ChallengeManager>();

        public readonly Dictionary<int, List<GameConditionDef>> ConditionsCreatedByEvent = new Dictionary<int, List<GameConditionDef>>();
        

        private Dictionary<ChallengeDef, int> QueuedAssaults => _queuedAssaults ??= new Dictionary<ChallengeDef, int>();
        private Dictionary<ChallengeDef, int> _queuedAssaults;
        private readonly List<ChallengeDef> _tmpChallengesToDo = new List<ChallengeDef>();
        private List<ChampionSpawnData> QueuedWaves => _queuedChampions ??= new List<ChampionSpawnData>();
        private List<ChampionSpawnData> _queuedChampions;
        private List<ChampionTrackerData> ActiveChampions => _activeChampions ??= new List<ChampionTrackerData>();
        private List<ChampionTrackerData> _activeChampions = new List<ChampionTrackerData>();
        

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref _activeChampions, "activeChampions", LookMode.Deep);
            Scribe_Collections.Look(ref _queuedAssaults, "queuedAssaults", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref _queuedChampions,  "queuedChampions", true, LookMode.Deep);
            base.ExposeData();
        }

        public void RegisterActiveChampion(int champion, ChallengeDef challenge)
        {
            ActiveChampions.Add(new ChampionTrackerData(champion, challenge));
            HealthBar.ChallengeDef = challenge;
            HealthBar.CurrentPawn = champion;
            Find.WindowStack.Add(HealthBar); 
            HealthBar.windowRect.y = 30;
            HealthBar.windowRect.x = Current.Camera.scaledPixelWidth / (float)2 - 1000;
        }
        
        public void RemoveActiveChampion(int champion)
        {
            ActiveChampions.RemoveWhere(data => data?.Champion == champion || data == null);
            if (ActiveChampions.Empty())
            {
                HealthBar.Close();
            }
        }

        public bool IsAnyAssaultQueued => !_queuedAssaults.NullOrEmpty();

        public ChallengeDef FirstQueuedAssault => _queuedAssaults.NullOrEmpty() ? null : _queuedAssaults.First().Key;

        /// <summary>
        /// Cannot start a challenge if its already queued
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public bool CanStartNewChallenge(ChallengeDef def)
        {
            return !QueuedAssaults.ContainsKey(def);
        }

        
        public void QueueAssault(ChallengeDef def)
        {
            QueuedAssaults[def] = Find.TickManager.TicksGame + def.ticksUntilArrival.RandomInRange;
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            int tickNow = Find.TickManager.TicksGame;
            if (tickNow % 200 != 0) return;

            if (ActiveChampions.Any())
            {
                if (!HealthBar.IsOpen)
                {
                    ChampionTrackerData championData = ActiveChampions.First();

                    if (championData == null)
                    {
                        ActiveChampions.RemoveAt(0);
                    }
                    else
                    {
                        HealthBar.ChallengeDef = championData.Challenge;
                        HealthBar.CurrentPawn = championData.Champion;
                        Find.WindowStack.Add(HealthBar); 
                        HealthBar.windowRect.y = 30;
                    }
                }
            }
            else
            {
                HealthBar.Close();
            }
            
            
            _tmpChallengesToDo.Clear();
            
            foreach (var (challengeDef, tick) in QueuedAssaults)
            {
                if (tickNow <= tick) continue;
                challengeDef.SpawnWave(0);
                if (challengeDef.waves.Count > 1)
                {
	                QueuedWaves.Add(new ChampionSpawnData(Find.TickManager.TicksGame + challengeDef.ticksBetweenWaves, challengeDef, 1));
                }
                _tmpChallengesToDo.Add(challengeDef);
            }

            foreach (ChallengeDef challengeDef in _tmpChallengesToDo)
            {
                QueuedAssaults.Remove(challengeDef);
            }

            if (QueuedWaves.NullOrEmpty()) return;
            
            var data = QueuedWaves.First();
            
            if (data == null)
            {
                Log.Error("Champion spawn data was null. Champion cannot spawn.");
                QueuedWaves.RemoveAt(0);
                return;
            }
            
            if (tickNow <= data.TickToSpawn) return;
            
            //Spawn the wave
            data.ChallengeDef.SpawnWave(data.Wave);
            
            //And increment the wave system
            data.Wave += 1;
            data.TickToSpawn += data.ChallengeDef.ticksBetweenWaves;
            
            //And if we have no more waves, then be rid of it
            if (data.ChallengeDef.waves.Count >= data.Wave)
            {
	            QueuedWaves.Remove(data);
            }
        }

        public void StartAllAssaults()
        {
            List <ChallengeDef> challenges = QueuedAssaults.Keys.ToList();
            foreach (var challengeDef in challenges)
            {
                QueuedAssaults[challengeDef] = Find.TickManager.TicksGame;
            }
        }
    }

    public class ChampionSpawnData : IExposable
    {
        public ChampionSpawnData()
        {
            
        }
        
        public ChampionSpawnData(int tickToSpawn, ChallengeDef challengeDef, int wave)
        {
            TickToSpawn = tickToSpawn;
            ChallengeDef = challengeDef;
            Wave = wave;
        }
        
        public int TickToSpawn;
        public ChallengeDef ChallengeDef;
        public int Wave;
        public void ExposeData()
        {
            Scribe_Values.Look(ref TickToSpawn, "ticksToSpawn");
            Scribe_Defs.Look(ref ChallengeDef, "challengeDef");
            Scribe_Values.Look(ref Wave, "wave");
        }
    }

    public class ChampionTrackerData : IExposable
    {
        public ChampionTrackerData()
        {
            
        }
        
        public ChampionTrackerData(int champion, ChallengeDef challenge)
        {
            Champion = champion;
            Challenge = challenge;
        }

        public int Champion;
        public ChallengeDef Challenge;
        public void ExposeData()
        {
            Scribe_Values.Look(ref Champion, "champion");
            Scribe_Defs.Look(ref Challenge, "challenge");
        }
    }
}