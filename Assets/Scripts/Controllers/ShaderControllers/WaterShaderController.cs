using UnityEngine;

public class WaterShaderController : MonoBehaviour
{
    private Material WaterShaderMat;

    #region Shader Property ID's

    private readonly int _WaveMagnitude = Shader.PropertyToID("WaveMagnitude");
    private readonly int _Speed = Shader.PropertyToID("Speed");
    private readonly int _Scale = Shader.PropertyToID("Scale");
    private readonly int _Offset = Shader.PropertyToID("_Offset");
    private readonly int _LengthScale = Shader.PropertyToID("LengthScale");
    private readonly int _AttenutationPower = Shader.PropertyToID("AttenuationPower");
    private readonly int _NoiseScale = Shader.PropertyToID("NoiseScale");
    private readonly int _NoiseSpeed = Shader.PropertyToID("NoiseSpeed");
    private readonly int _NoiseMagnitude = Shader.PropertyToID("NoiseMagnitude");


    #endregion


    #region Data Variables
    [SerializeField] private float WaveMagnitude;
    [SerializeField] private float Speed;
    [SerializeField] private float Scale;
    [SerializeField] private float Offset;
    [SerializeField] private float LengthScale;
    [SerializeField] private float AttenutationPower;
    [SerializeField] private float NoiseScale;
    [SerializeField] private float NoiseSpeed;
    [SerializeField] private float NoiseMagnitude;
    #endregion


    void Awake()
    {
        Renderer myRend  = GetComponent<Renderer>();
        WaterShaderMat = myRend.material;
        myRend.material = WaterShaderMat;
        //?

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter(Collider other)
    {
        Vector3 newOffsetPos = other.gameObject.transform.position - transform.position;
        Debug.Log($"New Offset {newOffsetPos}, Obj world pos {other.gameObject.transform.position}");
        // _Offset is 2D - but we don't care about height.
        // Use the Z value of newOffsetPos in a 2D vec

        // I won't pretend like I know why you have to inverse the vector.
        Vector2 projectedOffset = new Vector2(newOffsetPos.x, newOffsetPos.z) * -1;
        WaterShaderMat.SetVector(_Offset, projectedOffset);
    }

}
