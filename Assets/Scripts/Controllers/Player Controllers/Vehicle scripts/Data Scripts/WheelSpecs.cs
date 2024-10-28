using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wheel Specs", menuName = "ScriptableObjects/WheelSpecs", order = 1)]
public class WheelSpecs : ScriptableObject
{
    #region Variables

    public Transform _transform;

    [Header("Basic Setup")]
    public Transform[] Tires;
    public LayerMask _groundLayers;
    public bool debugRaycasts = true;
    public float tireRaycastDistance = 1.5f;

    //Accelerations

    [Header("Acceleration Setup")]
    [Tooltip("Top speed of the car")]
    public float vehicleTopSpeed = 500f;
    [Tooltip("How fast the car accelerates")]
    public float accelerationAmount = 3500f;
    public float baseAccelerationAmount;

    //suspension

    [Header("Suspension Setup")]

    [Tooltip("The force at which the spring tries to return to rest distanc with")]
    public float springStrength = 1000f;

    [Tooltip("Dampens speed at which spring returns to rest. Lower is bouncy, higher is stiff")]
    public float springDamping = 70f;

    [Tooltip("Distance in unity units the springs rest below the tire")]
    public float springRestDistance = 0.8f;

    //Breaking

    [Header("Breaking Setup")]
    public float tireMass = 10f;
    public bool isDrifting = false;

    [Tooltip("Determines If the Back wheels steer the car. If front and back true, all wheels turn. NOTE back wheels are the last two elements of the Tires array.")]

    public float _rotationAngleTimeToZero = 1.5f;
    public AnimationCurve _tireGripCurve;
    public float _tireGripHackFix = 100f;
    public float _tireTurnSpeed = 1f;

    #endregion
}
