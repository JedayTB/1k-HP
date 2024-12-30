using System.Collections;
using UnityEngine;

public class DynamicCameraManager : MonoBehaviour
{
  [SerializeField] Transform[] _cameraPositions;
  [SerializeField] int cameraPositionIndex;
  [SerializeField] float panTime = 1.5f;
  [SerializeField] private Camera _camera;

  //Just to make sure We're always in a valid pos and rotation
  private void Start()
  {
    MoveToLocation(0);
  }
  public void MoveToLocation(int characterCamPosIndex)
  {
    Debug.Log($"{this.gameObject.name} is moving to index {characterCamPosIndex}\n Pos{_cameraPositions[characterCamPosIndex].position}");
    Transform newTransform = _cameraPositions[characterCamPosIndex];

    cameraPositionIndex = characterCamPosIndex;
    // Stop Coroutine if running Then Start for next.
    // To fix camera panning if switching between locations 
    // Before pan was finished
    StopAllCoroutines();
    StartCoroutine(PanCameraToLocation(panTime, newTransform));
  }

  IEnumerator PanCameraToLocation(float panTime, Transform newTransform)
  {
    float count = 0;
    Vector3 cameraStartPos = _camera.transform.position;
    Quaternion cameraStartRotation = _camera.transform.rotation;
    Vector3 destPos = newTransform.position;
    Quaternion destRot = newTransform.rotation;

    float progress = 0;

    while (count < panTime)
    {
      count += Time.deltaTime;

      progress = LerpAndEasings.ExponentialDecay(progress, 1f, 5, Time.deltaTime);

      _camera.transform.position = Vector3.Slerp(cameraStartPos, destPos, progress);
      _camera.transform.rotation = Quaternion.Slerp(cameraStartRotation, destRot, progress);

      yield return null;
    }
  }
}
