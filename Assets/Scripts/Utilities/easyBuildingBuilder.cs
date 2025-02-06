using UnityEngine;

public class easyBuildingBuilder : MonoBehaviour
{
  [Header("Debug Options")]
  [SerializeField] private bool showGizmos = true;
  [SerializeField] private bool showSpawnLocations = true;
  [SerializeField] private float spawnLocationsSize = 5f;
  [SerializeField] private Color gizmoColor;

  [Header("Builing Spawning")]
  [SerializeField] private Transform WorldGeoTransform;
  [SerializeField] private GameObject[] buildingsList;
  [SerializeField] private Transform[] spawnLocations;

  [SerializeField] private GameObject[] spawnedBuildings;
  private void Awake()
  {
    refreshSpawnLocations();
  }
  public void createBuilding()
  {
    int rndIndex;
    deleteBuildings();

    spawnedBuildings = new GameObject[spawnLocations.Length];


    for (int i = 0; i < spawnLocations.Length; i++)
    {
      rndIndex = UnityEngine.Random.Range(0, buildingsList.Length - 1);


      GameObject tempBL = Instantiate(buildingsList[rndIndex]);

      tempBL.transform.parent = WorldGeoTransform;
      tempBL.transform.position = spawnLocations[i].position;

      //tempBL.layer = GameStateManager.Instance.CarPhysicsLayer;
      //tempBL.transform.rotation = spawnLocations[rndIndex].rotation;

      spawnedBuildings[i] = tempBL;
    }
  }
  public void deleteBuildings()
  {
    if (spawnedBuildings.Length == 0) return;

    for (int i = 0; i < spawnedBuildings.Length; i++)
    {
      if (spawnedBuildings[i] != null)
      {
        spawnedBuildings[i].transform.parent = this.transform;
        DestroyImmediate(spawnedBuildings[i].gameObject);
      }
      else
      {
        Debug.Log($"item at index {i} is null. Breaking for loop");
        break;
      }

    }

    spawnedBuildings = new GameObject[0];

  }


  void OnDrawGizmos()
  {
    if (showGizmos == false) return;
    refreshSpawnLocations();
    if (spawnLocations.Length == 0) return;
    drawSpawnLocations();
  }
  private void refreshSpawnLocations()
  {
    Transform[] potentialWaypoints = GetComponentsInChildren<Transform>();
    spawnLocations = new Transform[potentialWaypoints.Length - 1];

    for (int i = 1; i < potentialWaypoints.Length; i++)
    {
      spawnLocations[i - 1] = potentialWaypoints[i];
      spawnLocations[i - 1].name = $"Spawn Location {i}";
    }
  }
  private void drawSpawnLocations()
  {

    Vector3 lastWaypointposition = spawnLocations[^1].position;

    for (int i = 0; i < spawnLocations.Length; i++)
    {
      Transform temp = spawnLocations[i];

      Gizmos.DrawSphere(temp.position, spawnLocationsSize);
    }
  }
}
