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

        public ChallengeDef QueuedPhaseOne => _queuedPhaseOne;

        private ChallengeDef _queuedPhaseOne;
        private int _queuedPhaseOneTick = -1;

        private Dictionary<ChallengeDef, int> QueuedPhaseTwos => _queuedPhaseTwos ??= new Dictionary<ChallengeDef, int>();
        private Dictionary<ChallengeDef, int> _queuedPhaseTwos;
        private readonly List<ChallengeDef> _tmpChallengesToDo = new List<ChallengeDef>();
        private List<ChampionSpawnData> QueuedChampions => _queuedChampions ??= new List<ChampionSpawnData>();
        private List<ChampionSpawnData> _queuedChampions;
        private List<ChampionTrackerData> ActiveChampions => _activeChampions ??= new List<ChampionTrackerData>();
        private List<ChampionTrackerData> _activeChampions = new List<ChampionTrackerData>();
        

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref _activeChampions, "activeChampions", LookMode.Deep);
            Scribe_Defs.Look(ref _queuedPhaseOne, "queuedPhaseOne");
            Scribe_Values.Look(ref _queuedPhaseOneTick, "queuedPhaseOneTick");
            Scribe_Collections.Look(ref _queuedPhaseTwos, "queuedPhaseTwos", LookMode.Def, LookMode.Value);
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

        public bool IsPhaseOneQueued => _queuedPhaseOne != null;


        /// <summary>
        /// Cannot start a challenge if any phase 1 is queued, or its phase 2 is queued
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public bool CanStartNewChallenge(ChallengeDef def)
        {
            if (IsPhaseOneQueued)
            {
                return false;
            }

            return !QueuedPhaseTwos.ContainsKey(def);
        }

        public void StartPhaseOne(ChallengeDef def)
        {
            _queuedPhaseOne = def;
            _queuedPhaseOneTick = Find.TickManager.TicksGame + def.ticksUntilArrival.RandomInRange;
        }
        
        public void StartPhaseTwo(ChallengeDef def)
        {
            QueuedPhaseTwos[def] = Find.TickManager.TicksGame + def.ticksUntilRevenge.RandomInRange;
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
            
            
            if (IsPhaseOneQueued && tickNow > _queuedPhaseOneTick)
            {
                _queuedPhaseOne.FirePhaseOne();
                QueuedChampions.Add(new ChampionSpawnData(Find.TickManager.TicksGame + _queuedPhaseOne.ticksUntilChampionArrives, false, _queuedPhaseOne));
                _queuedPhaseOne = null;
            }
            
            _tmpChallengesToDo.Clear();
            
            foreach (var (challengeDef, tick) in QueuedPhaseTwos)
            {
                if (tickNow <= tick) continue;
                challengeDef.FirePhaseTwo();
                QueuedChampions.Add(new ChampionSpawnData(Find.TickManager.TicksGame + challengeDef.ticksUntilChampionArrives, true, challengeDef));
                _tmpChallengesToDo.Add(challengeDef);
            }

            foreach (ChallengeDef challengeDef in _tmpChallengesToDo)
            {
                QueuedPhaseTwos.Remove(challengeDef);
            }

            if (QueuedChampions.NullOrEmpty()) return;
            
            var data = QueuedChampions.First();
            
            if (data == null)
            {
                Log.Error("Champion spawn data was null. Champion cannot spawn.");
                QueuedChampions.RemoveAt(0);
                return;
            }
            
            if (tickNow <= data.TickToSpawn) return;
            
            data.ChallengeDef.SpawnChampion(data.IsPhaseTwo, data.IsPhaseTwo? data.ChallengeDef.championDrop: null);
            
            QueuedChampions.Remove(data);
        }

        public void StartAllPhaseTwos()
        {
            List <ChallengeDef> challenges = QueuedPhaseTwos.Keys.ToList();
            foreach (var challengeDef in challenges)
            {
                QueuedPhaseTwos[challengeDef] = Find.TickManager.TicksGame;
            }
        }
    }

    public class ChampionSpawnData : IExposable
    {
        public ChampionSpawnData()
        {
            
        }
        
        public ChampionSpawnData(int tickToSpawn, bool isPhaseTwo, ChallengeDef challengeDef)
        {
            TickToSpawn = tickToSpawn;
            IsPhaseTwo = isPhaseTwo;
            ChallengeDef = challengeDef;
        }
        
        public int TickToSpawn;
        public bool IsPhaseTwo;
        public ChallengeDef ChallengeDef;
        public void ExposeData()
        {
            Scribe_Values.Look(ref TickToSpawn, "ticksToSpawn");
            Scribe_Values.Look(ref IsPhaseTwo, "isPhaseTwo");
            Scribe_Defs.Look(ref ChallengeDef, "challengeDef");
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