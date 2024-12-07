using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GrandPrixManager
{
    public static int GameMode = 0; // 0 is Grand Prix, 1 is Level Select
    public static string[] LevelOrder = {"CityLevel", "MountainLevel", "PastelLevel", "JapanLevel"};
    public static string[] LevelDisplayNames = { "City", "Mountain", "Pastel City", "Japan"};
    public static int[] RacePlacements = { 0, 0, 0, 0 };
    public static int CurrentLevelIndex = 0;
    public static int GrandPrixLength = 2;

    public static void SetRacePlacement(int raceIndex, int racePlacement)
    {
        RacePlacements[raceIndex] = racePlacement;
    }
}
