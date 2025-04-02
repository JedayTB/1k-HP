using System.Collections;
using Unity.Burst.CompilerServices;
using UnityEngine;

public enum CameraPanType
{
  panToLocalPos,
  panToGlobalPos,
}
public class DynamicCameraManager : MonoBehaviour
{
  [SerializeField] Transform[] _cameraPositions;
  [SerializeField] CameraPanType panType = CameraPanType.panToGlobalPos;
  [SerializeField] int cameraPositionIndex;
  [SerializeField] float panTime = 1.5f;
  [SerializeField] private Camera _camera;
  [Header("Constant Lerp")]
  [SerializeField] float decayTime = 2.5f;

  //Just to make sure We're always in a valid pos and rotation
  private void Start()
  {
    MoveToLocation(0);
  }
  public void MoveToLocation(int characterCamPosIndex)
  {
    //Debug.Log($"{this.gameObject.name} is moving to index {characterCamPosIndex}\n Pos{_cameraPositions[characterCamPosIndex].position}");
    Transform newTransform = _cameraPositions[characterCamPosIndex];

    cameraPositionIndex = characterCamPosIndex;
    // Stop Coroutine if running Then Start for next.
    // To fix camera panning if switching between locations 
    // Before pan was finished
    StopAllCoroutines();
    switch (panType)
    {
      case CameraPanType.panToGlobalPos:
        StartCoroutine(PanCameraToGlobalLocation(panTime, newTransform));
        break;
      case CameraPanType.panToLocalPos:
        StartCoroutine(PanCameraToLocalLocation(panTime, newTransform));
        break;
    }
  }
  IEnumerator constantLerpTo(Transform lerpPos)
  {
    Vector3 camPos = _camera.transform.position;
    while (true)
    {
      camPos = _camera.transform.position;

      camPos.x = LerpAndEasings.ExponentialDecay(camPos.x, lerpPos.transform.position.x, decayTime, Time.deltaTime);
      camPos.y = LerpAndEasings.ExponentialDecay(camPos.y, lerpPos.transform.position.y, decayTime, Time.deltaTime);
      camPos.z = LerpAndEasings.ExponentialDecay(camPos.z, lerpPos.transform.position.z, decayTime, Time.deltaTime);

      _camera.transform.position = camPos;
      yield return null;
    }
  }

  IEnumerator PanCameraToGlobalLocation(float panTime, Transform newTransform)
  {
    float count = 0;
    Vector3 cameraStartPos = _camera.transform.position;
    Quaternion cameraStartRotation = _camera.transform.rotation;
    
    

    float progress = 0;

    while (count < panTime)
    {
      count += Time.deltaTime;

      progress = LerpAndEasings.ExponentialDecay(progress, 1f, 5, Time.deltaTime);

      _camera.transform.position = Vector3.Slerp(cameraStartPos, newTransform.position, progress);
      _camera.transform.rotation = Quaternion.Slerp(cameraStartRotation, newTransform.rotation, progress);

      yield return null;
    }
    StartCoroutine(constantLerpTo(newTransform));
  }
  IEnumerator PanCameraToLocalLocation(float panTime, Transform newTransform)
  {
    float count = 0;
    Vector3 cameraStartPos = _camera.transform.localPosition;
    Quaternion cameraStartRotation = _camera.transform.rotation;

    Vector3 destPos = newTransform.localPosition;
    Quaternion destRot = newTransform.rotation;

    float progress = 0;

    while (count < panTime)
    {
      count += Time.deltaTime;

      progress = LerpAndEasings.ExponentialDecay(progress, 1f, 5, Time.deltaTime);

      _camera.transform.localPosition = Vector3.Slerp(cameraStartPos, destPos, progress);
      _camera.transform.rotation = Quaternion.Slerp(cameraStartRotation, destRot, progress);

      yield return null;
    }
  }
}
