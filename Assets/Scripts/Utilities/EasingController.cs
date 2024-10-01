using UnityEngine;

public enum MovementType
{
    Linear,
    EaseInQuad,
    EaseOutQuad,
    EaseInOutQuad,
    EaseOutBounce
}

public class EasingController : MonoBehaviour
{
    public MovementType movementType = MovementType.Linear;
    public float durationInSeconds = 3.0f;

    public Vector3 startPosition = Vector3.zero;
    public Vector3 endPosition = Vector3.zero;

    public bool useCurrentPositionAsStart = false;
    public bool relativeEndPosition = false;
    public bool pingPong = false;

    private float currentDuration = 0f;

    private void Start()
    {
        if (useCurrentPositionAsStart) startPosition = transform.position;
        
        if (relativeEndPosition) endPosition += startPosition;
    }

    private void Update()
    {
        if (currentDuration < durationInSeconds)
        {
            currentDuration += Time.deltaTime;

            float t = 0f;

            switch(movementType)
            {
                case MovementType.Linear:
                    t = currentDuration / durationInSeconds;
                    break;
                case MovementType.EaseInQuad:
                    t = Mathf.Pow(currentDuration / durationInSeconds, 2);
                    break;
                case MovementType.EaseOutQuad:
                    t = 1 - Mathf.Pow(1 - currentDuration / durationInSeconds, 2);
                    break;
                case MovementType.EaseInOutQuad:
                    t = currentDuration / durationInSeconds;
                    if (t < 0.5f)
                    {
                        t = 2 * t * t;
                    }
                    else
                    {
                        t = 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
                    }
                    break;
                case MovementType.EaseOutBounce:
                    t = EaseOutBounce(currentDuration / durationInSeconds);
                    break;
            }

            //transform.localRotation = Quaternion.Euler(startPosition + (endPosition - startPosition) * t);
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
        }
        else if (pingPong)
        {
            (startPosition, endPosition) = (endPosition,  startPosition);
            currentDuration = 0f;
        }
    }

    private float EaseOutBounce(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (t < 1 / d1)
        {
            return n1 * t * t;
        }
        else if (t < 2 / d1)
        {
            return n1 * (t -= 1.5f / d1) * t + 0.75f;
        }
        else if (t < 2.5 / d1)
        {
            return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        }
        else
        { 
            return n1 * (t -= 2.625f / d1) * t + 0.983275f; 
        }
    }

}
