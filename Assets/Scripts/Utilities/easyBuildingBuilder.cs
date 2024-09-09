using UnityEngine;

public class easyBuildingBuilder : MonoBehaviour
{
    [SerializeField] private GameObject _buildingPrefab;
    [SerializeField] private GameObject roofPrefab;
    [SerializeField] private int floorsHigh;

    [SerializeField]private Vector3 _position;
    private Vector3 _incHeightPos;
    private Quaternion _rotation;
    public void createBuilding()
    {
        _position = transform.position;
        _rotation = transform.rotation;

        _incHeightPos = _position;
        for(int i = 0; i < floorsHigh -1; i++){
            _incHeightPos.y = i * 5;
            GameObject tempFloor = Instantiate(_buildingPrefab);
            tempFloor.transform.SetPositionAndRotation(_incHeightPos, _rotation);
            tempFloor.transform.SetParent(transform);
        }
    }
}
