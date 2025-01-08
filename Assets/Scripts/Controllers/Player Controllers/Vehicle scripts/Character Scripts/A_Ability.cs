using UnityEngine;

public abstract class A_Ability : MonoBehaviour
{
  public A_VehicleController vehicle;
  public AbilityAction onAbility;
  public virtual void AbilityUsed()
  {
    Debug.Log("Generic ability used");
  }
}
