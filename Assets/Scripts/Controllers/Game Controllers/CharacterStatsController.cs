using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;


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
  [SerializeField] private AnimationCurve SliderAnimCurve;
  [SerializeField] private float sliderAnimTime = 0.75f;
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
    StopAllCoroutines();
    StartCoroutine(lerpSliderValues(sliderAnimTime, charStat));
  }
  IEnumerator lerpSliderValues(float animTime, characterStats charStat)
  {
    float count = 0f;
    float progress = 0f;

    float startHP = horsePowerSlider.value;
    float startnitroChargeAmt = NitroChargeAmountSlider.value;
    float startHandling = handlingSlider.value;

    float endHp = charStat.HorsePower;
    float endNitro = charStat.NitroChargeAmounts;
    float endHandling = charStat.Handling;

    while (count < animTime)
    {
      count += Time.deltaTime;
      progress = count / animTime;

      float lerpVal = SliderAnimCurve.Evaluate(progress);

      horsePowerSlider.value = Mathf.Lerp(startHP, endHp, lerpVal);
      NitroChargeAmountSlider.value = Mathf.Lerp(startnitroChargeAmt, endNitro, lerpVal);
      handlingSlider.value = Mathf.Lerp(startHandling, endHandling, lerpVal);

      yield return null;
    }
  }
}

