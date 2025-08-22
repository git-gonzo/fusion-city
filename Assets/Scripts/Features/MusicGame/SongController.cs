using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SongController : MonoBehaviour
{
    public static SongController Instance => instance;
    public static float SongSpeed => instance.Config.speed;
    private static SongController instance;
    //To control how many times have played in a day
    [HideInInspector] public SpeedBoardTries musicTries = new SpeedBoardTries();

    public SongConfig Config;
    public AudioSource music;
    public AudioSource lobbyMusic;
    public Transform notesContainer;
    public LaneController lane1;
    public LaneController lane2;
    public LaneController lane3;
    public SongUI songUI;
    public List<int> playPrices;
    public bool isPlaying => _isPlaying;
    public bool isGameOver => _gameOver;

    private int _chunkIndex;
    private int _stepIndex;
    private float currentTime = 0f;
    private float totalTime = 0f;
    private float spawnTimer = 0f;
    private SongScore Score;

    private bool _isPlaying = false;
    private bool _allSpawned = false;
    private bool _gameOver = true;
    private List<float> hitTimes = new();
    internal List<NoteController> notesPool;

    public int TodayPrice { get 
        {
            if (musicTries.todayTries >= playPrices.Count) return playPrices.Last();
            return playPrices[musicTries.todayTries]; 
        } }
    private void Start()
    {
        instance = this;
        lobbyMusic.Play();
        
        UpdateTimer();
        if (MyScenesManager.Instance != null)
        {
            MyScenesManager.Instance.HideScreen();
        }
        LoadTries();
    }

    private async void LoadTries()
    {
        var results = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { "MusicTries"}
            );
        foreach (var result in results)
        {
            if (result.Key == "MusicTries")
            {
                musicTries = result.Value.Value.GetAs<SpeedBoardTries>();
            }
        }
    }

    private void Update()
    {
        if (_gameOver) return;
        //Record hits
        if (Input.GetKeyDown(KeyCode.A))
        {
            hitTimes.Add(totalTime - Config.timeBeforeSound);
        }
        if (_allSpawned) 
        {
            if(!HasActiveNotes())
            {
                Debug.Log("GameOver");
                GameOver();
                return;

            }
        }

        if (!_isPlaying) return;
        
        currentTime += Time.deltaTime;
        totalTime += Time.deltaTime;

        if (_chunkIndex == Config.chunks.Count - 1)
        {
            _isPlaying = false;
            _allSpawned = true;
            for (var i = 0; i< hitTimes.Count; i++)
            {
                if (i > 0)
                {
                    Debug.Log($"{i} delay {hitTimes[i] - hitTimes[i-1]}");
                }
            }
            return;
        }

        // Si se ha seleccionado modo Time since start
        if (Config.useTimeSinceStart)
        {
            return;
        }

        // Si el tiempo transcurrido supera el tiempo de espera, crear una nueva nota y reiniciar el temporizador
        if (currentTime >= spawnTimer)
        {
            SpawnNote();
            UpdateIndex();
            currentTime = 0f;
            UpdateTimer();
        }  
    }

    private bool HasActiveNotes()
    {
        foreach(Transform noteT in notesContainer)
        {
            if (noteT.gameObject.activeInHierarchy) return true;
        }
        return false;
    }

    private async void GameOver()
    {
        _chunkIndex = 0;
        songUI.ShowStartButtons();
        _gameOver = true;
        var result = await Leaderboards.SendScore(LeaderboardID.songLeaderboard, Score.Score);
        songUI.ShowFinalScore(Score.Score);
        Debug.Log(result);
        music.DOFade(0, 1.5f).SetDelay(2f).OnComplete(() => music.Stop());
        lobbyMusic.Play();
        lobbyMusic.DOFade(1, 1f).SetDelay(3.5f);
    }

    public void StartMusicGame()
    {
        notesPool = new();
        if (Config.useTimeSinceStart)
        {
            SpawnSong();
        }
        lobbyMusic.DOFade(0,1f).OnComplete(()=>lobbyMusic.Stop());
        Score ??= new();
        Score.Reset();
        _isPlaying = true;
        _allSpawned = false;
        _gameOver = false;
        music.resource = Config.musicTrack;
        music.Play();
        music.Pause();
        music.DOFade(1, 0);
        DOVirtual.DelayedCall(Config.timeBeforeSound,()=>music.Play());
        hitTimes = new List<float>();
        spawnTimer = 0f;
        lane1.InitLane();
        lane2.InitLane();
        lane3.InitLane();
        
            musicTries.IncreasePlayedToday();
        CloudSaveTimesPlayed();
    }

    public async void CloudSaveTimesPlayed()
    {
        var data = new Dictionary<string, object> { { "MusicTries", musicTries } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }
    
    private void SpawnSong()
    {
        for (int i = 0; i < Config.chunks.Count; i++)
        {
            SpawnChunk(i);
        }
        
    }
    private async Task SpawnChunk(int chunkIndex)
    {
        await Task.Delay((int)(Config.chunks[chunkIndex].timeSinceStart * 1000));
        for (int i = 0; i < Config.chunks[chunkIndex].steps.Count; i++)
        {
            await Task.Delay((int)(Config.chunks[chunkIndex].steps[i].timing * 1000));
            SpawnNoteStep(chunkIndex,i);
        }
        _chunkIndex = chunkIndex;
        Debug.Log("Chunkindex " +  _chunkIndex + " spawned");
    }

    private void SpawnNoteStep(int chunk, int step)
    {
        if (Config.chunks[chunk].steps[step].noteLane1 != NoteType.Empty)
        {
            lane1.SpawnNote(Config.chunks[chunk].steps[step].noteLane1);
        }
        if (Config.chunks[chunk].steps[step].noteLane2 != NoteType.Empty)
        {
            lane2.SpawnNote(Config.chunks[chunk].steps[step].noteLane2);
        }
        if (Config.chunks[chunk].steps[step].noteLane3 != NoteType.Empty)
        {
            lane3.SpawnNote(Config.chunks[chunk].steps[step].noteLane3);
        }
    }


    public void SpawnNote()
    { 
        if(Config.chunks[_chunkIndex].steps[_stepIndex].noteLane1 != NoteType.Empty)
        {
            lane1.SpawnNote(Config.chunks[_chunkIndex].steps[_stepIndex].noteLane1);
        }
        if(Config.chunks[_chunkIndex].steps[_stepIndex].noteLane2 != NoteType.Empty)
        {
            lane2.SpawnNote(Config.chunks[_chunkIndex].steps[_stepIndex].noteLane2);
        }
        if(Config.chunks[_chunkIndex].steps[_stepIndex].noteLane3 != NoteType.Empty)
        {
            lane3.SpawnNote(Config.chunks[_chunkIndex].steps[_stepIndex].noteLane3);
        }
    }

    private void UpdateIndex()
    {
        _stepIndex++;
        if(_stepIndex >= Config.chunks[_chunkIndex].steps.Count)
        {
            _stepIndex = 0;
            _chunkIndex++;
        }
    }
    private void UpdateTimer()
    {
        if (_chunkIndex >= Config.chunks.Count) return;
        spawnTimer = Config.chunks[_chunkIndex].steps[_stepIndex].timing;
    }

    public void AddScore(ScorePerformance performance, int amount)
    {
        Score ??= new();
        songUI.TotalScore(Score.AddScore(amount));
        songUI.OnAddScore(performance, amount);
    }

    public int NotesCount()
    {
        return notesContainer.childCount;
    }

    public void BackToMap()
    {
        if(MyScenesManager.Instance != null)
        {
            MyScenesManager.Instance.ShowScreen();
            lobbyMusic.DOFade(0, 1f).OnComplete(() => lobbyMusic.Stop());
            DOVirtual.DelayedCall(0.3f,()=> SceneManager.LoadScene(1));
            return;
        }
        SceneManager.LoadScene(1);
    }
}

public class SongScore
{
    int _score;
    public int Score => _score;
    public int AddScore(int amount)
    {
        return _score += amount;
    }
    public void Reset()
    {
        _score = 0;
    }
}

public enum ScorePerformance
{
    Bad = 0,
    NotThatGood = 1,
    Meh = 2,
    Good = 3,
    NotBad = 4,
    VeryGood = 5,
    Awesome = 6,
    Perfect = 7
}
