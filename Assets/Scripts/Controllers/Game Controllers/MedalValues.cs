using UnityEngine;
[CreateAssetMenu(fileName = "Medal Value", menuName = "ScriptableObjects/Medal Value", order = 3)]
public class MedalValues : ScriptableObject
{
  public float AuthorLapTime = 0;
  public float subtwominute = 2 * 60;
  public float DevLapTime = 2.5f * 60f;
  public float PlatinumLapTime = 3f * 60f;
  public float GoldLapTime = 3.5f * 60f;
  public float SilverLapTime = 4f * 60f;
  public float BronzeLapTime = 4.5f * 60f;
  public float WoodLapTime = 5f * 60f;

  public float WhyDidYouGrindScore_Score = 600000;
  public float DevScore = 100000f;
  public float PlatinumScore = 75000f;
  public float GoldScore = 60000f;
  public float SilverScore = 50000f;
  public float BronzeScore = 25000f;
  public float WoodScore = 0f;
}
