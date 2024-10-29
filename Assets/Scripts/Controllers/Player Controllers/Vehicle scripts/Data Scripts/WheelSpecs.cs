using UnityEngine;

[CreateAssetMenu(fileName = "Wheel Specs", menuName = "ScriptableObjects/WheelSpecs", order = 1)]
public class WheelSpecs : ScriptableObject
{
    public float tireRaycastDistance = 1.5f;
    //suspension
    [Tooltip("The force at which the spring tries to return to rest distance with")]
    public float springStrength = 1000f;

    [Tooltip("Dampens speed at which spring returns to rest. Lower is bouncy, higher is stiff")]
    public float springDamping = 70f;

    [Tooltip("Distance in unity units the springs rest below the tire")]
    public float springRestDistance = 0.8f;
    public float tireMass = 10f;
}
