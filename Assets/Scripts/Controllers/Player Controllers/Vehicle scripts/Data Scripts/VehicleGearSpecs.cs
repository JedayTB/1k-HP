using UnityEngine;

[CreateAssetMenu(fileName = "Vehicle Gear Specs", menuName = "ScriptableObjects/VehicleGearSpecs", order = 2)]

public class VehicleGearSpecs : ScriptableObject
{
  [Tooltip("Horse Power at given gear")]
  [SerializeField] public float HorsePower = 500f;
  [Tooltip("Axle Efficiency at given gear")]
  [SerializeField] public float AxleEfficiency = 0.85f;
}
