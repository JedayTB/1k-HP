using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


struct characterStats
{
  public string characterName;
  public float HorsePower;
  public float Handling;
  public int NitroChargeAmounts;
}

public class CharacterStatsController : MonoBehaviour
{
  [SerializeField] A_VehicleController[] CharactersList;
  [SerializeField] Transform[] characterSpawnLocations;
  private characterStats[] charStats;

  [SerializeField] Slider horsePowerSlider;
  [SerializeField] Slider NitroChargeAmountSlider;
  [SerializeField] Slider handlingSlider;

  private float highestHP = float.MinValue;
  private float highestTurningRadius = float.MinValue;
  private float highestNitroChargeAmounts = int.MinValue;

  private Dictionary<string, characterStats> NameToStat = new();

  void Start()
  {
    charStats = new characterStats[CharactersList.Length];
    for (int i = 0; i < CharactersList.Length; i++)
    {
      var vehicle = Instantiate(CharactersList[i]);
      vehicle.Init();
      vehicle.transform.position = characterSpawnLocations[i].position;
      // Disable the controller
      vehicle.enabled = false;
    }


    for (int i = 0; i < CharactersList.Length; i++)
    {
      if (CharactersList[i].VehiclePhysics.horsePower > highestHP) highestHP = CharactersList[i].VehiclePhysics.horsePower;
      if (CharactersList[i].VehiclePhysics.turnRadius > highestTurningRadius) highestTurningRadius = CharactersList[i].VehiclePhysics.turnRadius;
      if (CharactersList[i].MaxNitroChargeAmounts > highestNitroChargeAmounts) highestNitroChargeAmounts = CharactersList[i].MaxNitroChargeAmounts;
      //
      Debug.Log(CharactersList[i].gameObject.name);

      // 
      //
      charStats[i] = new characterStats();

      charStats[i].characterName = CharactersList[i].gameObject.name;
      charStats[i].HorsePower = CharactersList[i].VehiclePhysics.horsePower;
      charStats[i].NitroChargeAmounts = CharactersList[i].MaxNitroChargeAmounts;
      charStats[i].Handling = CharactersList[i].VehiclePhysics.turnRadius;
      //  
      NameToStat.Add(CharactersList[i].gameObject.name, charStats[i]);

    }

    horsePowerSlider.maxValue = highestHP;
    handlingSlider.maxValue = highestTurningRadius;
    NitroChargeAmountSlider.maxValue = highestNitroChargeAmounts;
  }

  public void changeAllSliderValues(string characterName)
  {
    characterStats charStat = NameToStat[characterName];
    // Should put in quick Lerp but idgaf rn
    horsePowerSlider.value = charStat.HorsePower;
    NitroChargeAmountSlider.value = charStat.NitroChargeAmounts;
    handlingSlider.value = charStat.Handling;
  }
}

