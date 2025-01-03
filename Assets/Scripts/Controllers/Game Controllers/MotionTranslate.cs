using System.Collections;
using UnityEngine;

public class MotionTranslate : MonoBehaviour
{
    [SerializeField] private Vector3 moveDirection = new(0, 0, 1);
    [SerializeField] private float timeToMaxSpeed = 10f;
    [SerializeField] private float StartSpeed = 10f;

    [SerializeField] private float MaxSpeed = 40f;
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        speed = StartSpeed;
        moveDirection.Normalize();
        StartCoroutine(increaseSpeed(timeToMaxSpeed));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Translate(speed * Time.fixedDeltaTime * moveDirection);
    }
   
    IEnumerator increaseSpeed(float timeToMax)
    {
        float timeCount = 0f;
        while (timeCount < timeToMax)
        {
            timeCount += Time.deltaTime;
            float progress = timeCount / timeToMax;
            speed = Mathf.Lerp(StartSpeed, MaxSpeed, progress);
            yield return null;
        }
    }
}
