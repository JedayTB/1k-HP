using UnityEngine;

public class DONTFUCKINGCLIPOUT : MonoBehaviour
{
  public Vector3 addPos = new Vector3(0, 0.15f, 1);
  public float UpwardsForceonCollision = 500f;
  //other.transform.parent.parent.gameObject.tag == "PLAYER"
  void OnTriggerEnter(Collider other)
  {

    //Debug.Log($"{other.gameObject.name}");
    if (other.tag == "PLAYER")
    {
      Debug.Log("Get out of the ground you fucker - T");
      float rbmass = GameStateManager.Player.VehiclePhysics.RigidBody.mass;
      GameStateManager.Player.VehiclePhysics.RigidBody.AddForce(UpwardsForceonCollision * rbmass * Vector3.up);
      GameStateManager.Player.transform.position += addPos;
    }
  }
  void OnTriggerStay(Collider other)
  {
    if (other.tag == "PLAYER")
    {
      Debug.Log("Get out of the ground you fucker - T");
      float rbmass = GameStateManager.Player.VehiclePhysics.RigidBody.mass;
      GameStateManager.Player.VehiclePhysics.RigidBody.AddForce(UpwardsForceonCollision * rbmass * Vector3.up);
      GameStateManager.Player.transform.position += addPos;
    }
  }
  void OnColliderEnter(Collision other)
  {
    //Debug.Log($"{other.gameObject.name}");
    if (other.gameObject.tag == "PLAYER")
    {
      Debug.Log("Get out of the ground you fucker - C");
      float rbmass = GameStateManager.Player.VehiclePhysics.RigidBody.mass;
      GameStateManager.Player.VehiclePhysics.RigidBody.AddForce(UpwardsForceonCollision * rbmass * Vector3.up);
    }
  }
}
