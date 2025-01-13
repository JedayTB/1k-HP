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
  private characterStats[] charStats;

  [SerializeField] Slider horsePowerSlider;
  [SerializeField] Slider NitroChargeAmountSlider;
  [SerializeField] Slider handlingSlider;

  private float highestMaxSpeed = float.MinValue;
  private float highestHandling = float.MinValue;
  private float highestNitroChargeAmounts = int.MinValue;

  private Dictionary<string, characterStats> NameToStat = new();

  void Start()
  {
    charStats = new characterStats[CharactersList.Length];
    
    for (int i = 0; i < CharactersList.Length; i++)
    {
      var vehicle = CharactersList[i];
      vehicle.Init();
      // Disable the controller
      vehicle.enabled = false;
    }

    for (int i = 0; i < CharactersList.Length; i++)
    {
      float handlingAverage = CharactersList[i].VehiclePhysics.WheelArray[0].LeftAckermanAngle +
                              CharactersList[i].VehiclePhysics.WheelArray[0].RightAckermanAngle +
                              CharactersList[i].VehiclePhysics.WheelArray[1].LeftAckermanAngle +
                              CharactersList[i].VehiclePhysics.WheelArray[1].RightAckermanAngle;
      handlingAverage /= 4;

      if (CharactersList[i].VehiclePhysics.TerminalVelocity > highestMaxSpeed) highestMaxSpeed = CharactersList[i].VehiclePhysics.TerminalVelocity;
      if (handlingAverage > highestHandling) highestHandling = handlingAverage;
      if (CharactersList[i].MaxNitroChargeAmounts > highestNitroChargeAmounts) highestNitroChargeAmounts = CharactersList[i].MaxNitroChargeAmounts;
      //
      //
      charStats[i] = new characterStats();

      charStats[i].characterName = CharactersList[i].gameObject.name;
      charStats[i].HorsePower = CharactersList[i].VehiclePhysics.horsePower;
      charStats[i].NitroChargeAmounts = CharactersList[i].MaxNitroChargeAmounts;
      charStats[i].Handling = handlingAverage;
      //  
      NameToStat.Add(CharactersList[i].gameObject.name, charStats[i]);

    }

    horsePowerSlider.maxValue = highestMaxSpeed;
    handlingSlider.maxValue = highestHandling;
    NitroChargeAmountSlider.maxValue = highestNitroChargeAmounts;

    changeAllSliderValues(CharactersList[2].gameObject.name);
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

