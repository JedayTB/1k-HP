using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.VFX;
using TMPro;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    // Every 0.25 seconds, calculate Race placements
    // RECALCULATE RACE PLACEMENTS AT END OF RACE
    private static readonly float RACEPLACEMENTSTICK = 0.15f;
    private static readonly float MUSICCROSSFADETIME = 4f;
    [SerializeField] private PlayerVehicleController _player;
    public A_Ability[] Abilitieslist;
    
    private static GameStateManager instance;
    public static GameStateManager Instance { get { return instance; } }

    public bool UseDebug = true;
    [Header("Game Logic Values")]
    public int LapsToFinishRace = 3;
    public static float countdownTime = 3;
    [SerializeField] private MedalValues StageMedalValues;
    [Header("Game Logic Objects")]
    [SerializeField] private CameraFollower3D cam;
    [SerializeField] public LapChecker _lapChecker;
    [SerializeField] private LapTimer _lapTimer;
    [SerializeField] public UIController _uiController;
    [SerializeField] private VehicleAIController[] _aiControllers;
    [SerializeField] private GameObject[] _playerVehicles;
    [SerializeField] private Transform[] _startLocations;
    [SerializeField] private PostProcessing _postProcessing;
    [SerializeField] private MusicManager _musicManager;
    [SerializeField] private MusicManager EndLevelMusic;
    [SerializeField] private NodeCloudUtil NodeCloud;
    [Header("Cursor Sprites")]
    public Texture2D lightningCursor;

    [Header("Race Placements and debug")]
    [SerializeField] string[] vehicleDbgInfo;
    public int nextPlayerCheckpointPosition = 0;

    [Header("Text Fields")]
    // UI Stuff
    [SerializeField] private TextMeshProUGUI _lapTimesText;
    [SerializeField] private TextMeshProUGUI _totalTimeText;
    [SerializeField] private TextMeshProUGUI _rankText;

    [Header("Rank Colors")]
    [SerializeField] private Color DevMedalColor;
    [SerializeField] private Color PlatinumMedalColor;
    [SerializeField] private Color GoldMedalColor;
    [SerializeField] private Color SilverMedalColor;
    [SerializeField] private Color BronzeMedalColor;
    [SerializeField] private Color WoodColor;

    [Header("Misc")]
    [SerializeField] private VisualEffect SkidParticlesPrototype;
    private static readonly string skidParticlesVelocityID = "Velocity";
    public LayerMask CarPhysicsLayer;
    public static readonly float musicVolumeLevel = 0.5f;

    private InputManager inputManager;

    public static PlayerVehicleController Player; // Singleton var

    public static int _newCharacter = 0;

    // Hidden
    [HideInInspector] public List<A_VehicleController> vehicles = new List<A_VehicleController>();
    [HideInInspector]
    public Vector3[] LevelCheckpointLocations
    {
        get
        {
            return _lapChecker.checkPointLocations;
        }
    }

    private void Awake()
    {
        instance = this;
        if (_player == null)
        {
            _player = Instantiate(_playerVehicles[_newCharacter]).GetComponent<PlayerVehicleController>();
        }
        Player = _player;

        inputManager = this.gameObject.AddComponent<InputManager>();
        inputManager.Init();

        _lapChecker?.Init(this);
        _uiController?.init(_player);
        cam = Camera.main.gameObject.GetComponentInParent<CameraFollower3D>();
        cam?.Init(inputManager);

        // breaks at this one for some reason, had to move everything else up to stop them from not being called
        _player?.Init(inputManager);

        vehicles.Add(_player);

        int vehiclesToPosition = 1;

        _player.VehiclePhysics.RigidBody.constraints = RigidbodyConstraints.FreezePosition;

        if (_aiControllers.Length == 0)
        {
            VehicleAIController[] ais = FindObjectsByType<VehicleAIController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }
        for (int i = 0; i < _aiControllers.Length; i++)
        {
            if (_aiControllers[i].gameObject.activeSelf)
            { 
                vehicles.Add(_aiControllers[i]);
                _aiControllers[i].Init();
                vehiclesToPosition++;
                _aiControllers[i].VehiclePhysics.RigidBody.constraints = RigidbodyConstraints.FreezePosition;
            }
        }

        // Set checkpoints passed through.
        int checkpointsArrLength = LevelCheckpointLocations.Length;

        for (int i = 0; i < vehicles.Count; i++)
        {
            vehicles[i].checkpointsPassedThrough = new bool[checkpointsArrLength];
            // Set all to false
            for (int k = 0; k < checkpointsArrLength; k++)
            {
                vehicles[i].checkpointsPassedThrough[k] = false;
            }
        }

        // Set Vehicles position
        for (int i = 0; i < vehiclesToPosition; i++)
        {
            vehicles[i].transform.SetPositionAndRotation(_startLocations[i].position, _startLocations[i].rotation);
            vehicles[i].setNewRespawnPosition(_startLocations[i]);
        }
        _postProcessing?.init();
        StartCoroutine(calculateVehiclePlacements());
        Debug.Log("GSM has Finished Intializing!");
    }
    private void Start()
    {
        var speedlines = FindAnyObjectByType<SpeedLinesController>(FindObjectsInactive.Include);
        speedlines.enabled = true;
    }
    public void StartLapTimer()
    {
        _lapTimer?.StartTimingLaps();
    }
    IEnumerator calculateVehiclePlacements()
    {
        // Starting logic
        Dictionary<float, A_VehicleController> distPlayerDict = new();

        // Last index is distance of nth - 0th index
        float[] distancesBetweenCheckpoints = new float[LevelCheckpointLocations.Length];
        // Calculate distanes between checkpoints
        for (int i = 0; i < LevelCheckpointLocations.Length; i++)
        {
            int nextIndex = i + 1;
            if (nextIndex == LevelCheckpointLocations.Length) nextIndex = 0;
            distancesBetweenCheckpoints[i] = Vector3.Distance(LevelCheckpointLocations[i], LevelCheckpointLocations[nextIndex]);
        }

        float checkpointFraction = 1f / LevelCheckpointLocations.Length;
        float[] progressions = new float[vehicles.Count];
        vehicleDbgInfo = new string[vehicles.Count];

        while (true)
        {
            try
            {
                distPlayerDict.Clear();
                // Calculate Distances and progression
                for (int i = 0; i < vehicles.Count; i++)
                {

                    float distToNextCheckpoint = Vector3.Distance(vehicles[i].transform.position, LevelCheckpointLocations[vehicles[i].nextCheckpointIndex]);

                    float distProgression = Mathf.Clamp01(1 - (distToNextCheckpoint / distancesBetweenCheckpoints[vehicles[i].nextCheckpointIndex]));

                    float roundedProgression = (vehicles[i].nextCheckpointIndex - 1) * checkpointFraction;

                    float particleProgression = distProgression * checkpointFraction;

                    int amtOfLapsPassed = vehicles[i].lapsPassed;

                    float progression = roundedProgression + particleProgression + amtOfLapsPassed;


                    if (UseDebug)
                    {
                        Vector3 dir = LevelCheckpointLocations[vehicles[i].nextCheckpointIndex] - vehicles[i].transform.position;
                        Debug.DrawRay(vehicles[i].transform.position, dir, Color.white, RACEPLACEMENTSTICK);
                        vehicleDbgInfo[i] = $"{vehicles[i].name}: nxt chckpnt ind {vehicles[i].nextCheckpointIndex}: Place {vehicles[i].racePlacement} Laps {amtOfLapsPassed}\n"
                          + $"prog {progression}: round prog {roundedProgression}: part prog {particleProgression}: dist prog {distProgression}:\n"
                          + $"Next chkpnt dist {distToNextCheckpoint}";
                    }

                    distPlayerDict.Add(progression, vehicles[i]);
                    progressions[i] = progression;
                }

                // Sort distances and input placements based on that
                //
                int zeroIndexLn = vehicles.Count - 1;
                SortingAlgorithms.QuickSort(progressions, 0, zeroIndexLn);

                int count;
                for (int i = 0; i < progressions.Length; i++)
                {
                    count = zeroIndexLn - i + 1;

                    A_VehicleController vRef = distPlayerDict[progressions[i]];

                    vRef.racePlacement = count;
                }
            }
            catch (Exception)
            {
                //Debug.LogError($"{e.Message} \nSomehow two vehicles had the same progression. Dumb. Stupid. Idiot");
                distPlayerDict.Clear();
            }


            yield return new WaitForSeconds(RACEPLACEMENTSTICK);
        }

    }

    public void setVehicleNextCheckpoint(A_VehicleController vehicle, int index)
    {

        var pl = vehicle.GetComponent<PlayerVehicleController>();
        if (pl != null)
        {
            nextPlayerCheckpointPosition = index;
            pl.nextCheckpointIndex = index;
        }
        else
        {
            // Don't think this matters... But Set it in case!
            vehicle.nextCheckpointIndex = index;
        }
    }
    public void setVehicleLapCount(A_VehicleController vehicle)
    {
        vehicle.lapsPassed++;
        for (int i = 0; i < vehicle.checkpointsPassedThrough.Length; i++)
        {
            vehicle.checkpointsPassedThrough[i] = false;
        }
    }
    public void onPlayerWin()
    {
        Debug.Log("Player won!");

        _uiController.setPlayScreen(false);

        List<string> lapTimes = _lapTimer.lapTimes;
        string lapTimesStr = "";

        //Because List's don't have a defined size?
        int count = 0;

        float totalTime = 0;

        foreach (var lapTime in lapTimes)
        {
            totalTime += _lapTimer.lapTimesNumbers[count];
            lapTimesStr += $"Lap {count + 1} Time: {lapTime}\n";
            count++;
        }

        calculateEndTimeRank(totalTime);

        print(lapTimesStr);

        _lapTimesText.text = lapTimesStr;

        bool isGP = GrandPrixManager.GameMode == 0 ? true : false;
        GrandPrixManager.SetRacePlacement(GrandPrixManager.CurrentLevelIndex, 1);
        GrandPrixManager.CurrentLevelIndex += isGP ? 1 : 0;
        _uiController.setWinScreen(true, isGP);


        StartCoroutine(CrossFadeLevelAndEndMusic(MUSICCROSSFADETIME));
    }
    private void calculateEndTimeRank(float totalTime)
    {
        float minutes = totalTime / 60;
        float seconds = totalTime % 60;

        _totalTimeText.text = $"{Mathf.FloorToInt(minutes):00}:{seconds:00.00}";
        SetRankText(totalTime);
    }
    private void SetRankText(float totalTime)
    {
        string setText = "";
        Color setColor = Color.white;
        if (totalTime < StageMedalValues.DevLapTime)
        {
            setText = "Dev";
            setColor = DevMedalColor;
        }
        else if (totalTime < StageMedalValues.PlatinumLapTime)
        {
            setText = "Platinum";
            setColor = PlatinumMedalColor;
        }
        else if (totalTime < StageMedalValues.GoldLapTime)
        {
            setText = "Gold";
            setColor = GoldMedalColor;
        }
        else if (totalTime < StageMedalValues.SilverLapTime)
        {
            setText = "Silver";
            setColor = SilverMedalColor;
        }
        else if (totalTime < StageMedalValues.BronzeLapTime)
        {
            setText = "Bronze";
            setColor = BronzeMedalColor;
        }
        else
        {
            setText = "Wood. Wow.";
            setColor = WoodColor;
        }
        _rankText.text = setText;
        _rankText.color = setColor;
    }
    private IEnumerator CrossFadeLevelAndEndMusic(float crossfadetime)
    {
        float count = 0f;
        float currentmusicvolume = _musicManager.musicSource.volume;
        float progress = 0f;
        EndLevelMusic.musicSource.Play();
        while (count < crossfadetime)
        {
            count += Time.deltaTime;
            progress = count / crossfadetime;
            _musicManager.musicSource.volume = Mathf.Lerp(currentmusicvolume, 0f, progress);
            EndLevelMusic.musicSource.volume = Mathf.Lerp(0f, currentmusicvolume, progress);
            yield return null;
        }
    }


    public void UnfreezeAIs()
    {

        foreach (VehicleAIController ai in _aiControllers)
        {
            ai.VehiclePhysics.RigidBody.constraints = RigidbodyConstraints.None;
            // Just to reset velocity calculations
            ai.VehiclePhysics.RigidBody.velocity = Vector3.zero;
            ai.VehiclePhysics.setInputs(0, 0);
            ai._driveVehicle = true;
        }
    }
    public void spawnSkidParticles(Vector3 positionOfHit, Vector3 orientation, float velocityOfHit)
    {
        var skidparticles = Instantiate(SkidParticlesPrototype);

        Quaternion newRot = Quaternion.Euler(orientation);
        skidparticles.transform.SetPositionAndRotation(positionOfHit, newRot);

        skidparticles.SetFloat(skidParticlesVelocityID, velocityOfHit);
    }
}
