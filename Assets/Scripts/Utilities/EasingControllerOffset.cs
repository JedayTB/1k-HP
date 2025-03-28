using System.Collections;
using UnityEngine;

public class EasingControllerOffset : MonoBehaviour
{
  [SerializeField] private float minimumTimeOffset = 0f;
  [SerializeField] private float maximumTimeOffset = 0.75f;

  [SerializeField] private float minLerpTime = 5f;
  [SerializeField] private float maxLerpTime = 15f;

  EasingController eas;
  void Awake()
  {
    eas = GetComponent<EasingController>();
    if (eas != null)
    {
      eas.enabled = false;
      eas.durationInSeconds = Random.Range(minLerpTime, maxLerpTime);
      StartCoroutine(countDown());

    }
  }
  IEnumerator countDown()
  {
    float count = 0f;

    float waitTime = Random.Range(minimumTimeOffset, maximumTimeOffset);

    while (count < waitTime)
    {
      count += Time.deltaTime;
      yield return null;
    }
    eas.enabled = true;
  }
}
