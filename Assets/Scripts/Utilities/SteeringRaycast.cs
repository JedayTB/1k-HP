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
    public bool raycastForward(out RaycastHit hit, float distance, LayerMask Layer)
    {
        return Physics.Raycast(transform.position, transform.forward, out hit, distance, Layer);    
    }
    


    
}
