using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CarVisualController : MonoBehaviour
{
  // Wheel containers and wheels are separate
  // This is so we can have wheel yaw (left, right), and pitch(spin)
  // Ask Ethan Arr for further clarification

  [SerializeField] protected Vector3 spinWheelDirection;
  [SerializeField] protected Vector3 turnWheelDirection;
  [SerializeField] protected Transform[] _wheelContainers;
  [SerializeField] public Transform[] _wheelModels;
  [SerializeField] protected List<TrailRenderer> _trails;
  [SerializeField] protected ParticleSystem[] driftParticles;
  [SerializeField] protected DecalProjector blobProjector;
  [SerializeField] protected LayerMask playerLayer;
  protected CustomCarPhysics _vehiclePhysics;
  protected CustomWheels[] PhysicsWheels;
  protected Rigidbody _rb;

  // Car models have tires at different heights.
  // offsetTireWithSuspension must take that into consideration.
  protected float[] baseTireRestHeights;

  public virtual void Init()
  {
    _vehiclePhysics = GetComponent<CustomCarPhysics>();
    PhysicsWheels = _vehiclePhysics.WheelArray;
    _rb = _vehiclePhysics.RigidBody;

    createbaseTireRestHeights();
  }
  protected void createbaseTireRestHeights()
  {
    baseTireRestHeights = new float[_wheelContainers.Length];
    for (int i = 0; i < _wheelContainers.Length; i++)
    {
      baseTireRestHeights[i] = _wheelContainers[i].transform.localPosition.y;
    }
  }
  /// <summary>
  /// Used as Update - But only when called!
  /// gives more control over the scripts functionality.
  /// </summary>
  public void Process()
  {
    for (int i = 0; i < _wheelContainers.Length; i++)
    {
      float velocityAtWheelPoint = _rb.GetPointVelocity(PhysicsWheels[i].transform.position).z;
      //Rotates the model
      SpinWheels(_wheelModels[i], velocityAtWheelPoint);

      //Rotates the container
      float rotAngle = PhysicsWheels[i].SteeringAngle;
      TurnWheels(_wheelContainers[i], rotAngle);
      offsetTireWithSuspension(_wheelContainers[i], i);
    }

    bool useDriftParticles = _vehiclePhysics.isDrifting;
    bool useTrails = _vehiclePhysics.isUsingNitro;

    emitDriftParticles(useDriftParticles);
    activateTrails(useTrails);

    BlobProjection();
  }
  protected void offsetTireWithSuspension(Transform visualWheel, int index)
  {
    Vector3 currentLocalPosition = visualWheel.localPosition;
    float yOffset = baseTireRestHeights[index] + PhysicsWheels[index].SuspensionOffset;
    Vector3 offsetPosition = new(currentLocalPosition.x, yOffset, currentLocalPosition.z);

    visualWheel.localPosition = offsetPosition;
  }
  protected void emitDriftParticles(bool on)
  {
    if (on)
    {
      for (int i = 0; i < driftParticles.Length; i++)
      {
        driftParticles[i].Emit(3);
      }
    }
  }
  public void activateTrails(bool on)
  {
    foreach (var trail in _trails)
    {
      trail.emitting = on;
    }
  }
  public void SpinWheels(Transform wheel, float wheelvelocity)
  {
    Vector3 rotation = spinWheelDirection * wheelvelocity;
  }
  public void TurnWheels(Transform wheel, float rotationAngle)
  {
    Vector3 currentRotation = wheel.localRotation.eulerAngles;

    Vector3 rotationEul = new(currentRotation.x, rotationAngle, currentRotation.z);

    Quaternion rotation = Quaternion.Euler(rotationEul);

    wheel.localRotation = rotation;
  }

  private void BlobProjection()
  {
    RaycastHit hit;
    Physics.Raycast(transform.position, Vector3.down, out hit, 10000, playerLayer);
    float yDifference = hit.distance;
    Vector3 newPivot = new Vector3(0, -yDifference, 0);
    blobProjector.pivot = newPivot;
    //        print(hit.collider.transform.name);
  }
}
