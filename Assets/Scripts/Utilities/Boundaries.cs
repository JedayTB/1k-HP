using UnityEngine;

public class Boundaries : MonoBehaviour
{
    public bool meshEnabled = true;
    public MeshRenderer meshRend;
    
    void OnValidate()
    {
        meshRend.enabled = meshEnabled;
    }
}
