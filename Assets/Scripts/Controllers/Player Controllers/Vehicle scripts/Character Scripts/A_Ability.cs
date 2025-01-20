using UnityEngine;

public abstract class A_Ability : MonoBehaviour
{
  public A_VehicleController vehicle;
  public AbilityAction onAbility;
  public virtual void AbilityUsed()
  {
    Debug.Log("Generic ability used");
  }
  protected virtual void Awake()
  {
    onAbility = AbilityUsed;
  }

  private void OnEnable()
  {
    //        print($"{this.name} ability actoin enlisted");
    vehicle.enlistAbilityAction(onAbility);
  }

  private void OnDisable()
  {
    vehicle.delistAbilityAction(onAbility);
  }
}
