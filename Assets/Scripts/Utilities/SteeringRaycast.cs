using UnityEngine;

public enum SteeringRaycastType
{
  frontal,
  leftAngled,
  rightAngled,
}
public class SteeringRaycast : MonoBehaviour
{
  public SteeringRaycastType rcType = SteeringRaycastType.frontal;
}
