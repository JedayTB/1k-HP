using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GrandPrixManager
{
    public static int GameMode = 0; // 0 is Grand Prix, 1 is Level Select
    public static string[] LevelOrder = {"CityLevel", "MountainLevel", "PastelLevel", "JapanLevel"};
    public static string[] LevelDisplayNames = { "City", "Mountain", "Pastel City", "Japan"};
    public static int CurrentLevelIndex = 0;

}
