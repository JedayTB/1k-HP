using UnityEngine;
[CreateAssetMenu(fileName = "Medal Value", menuName = "ScriptableObjects/Medal Value", order = 3)]
public class MedalValues : ScriptableObject 
{
   public float DevLapTime = 2.5f * 60f;
   public float PlatinumLapTime = 3f * 60f;
   public float GoldLapTime = 3.5f * 60f;
   public float SilverLapTime = 4f * 60f;
   public float BronzeLapTime = 4.5f * 60f;
   public float WoodLapTime = 5f * 60f;
}
