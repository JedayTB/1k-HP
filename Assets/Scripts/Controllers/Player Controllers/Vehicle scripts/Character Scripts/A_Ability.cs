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

  protected virtual void OnEnable()
  {
    print($"{this.name} ability action enlisted");
    vehicle.enlistAbilityAction(onAbility);
  }

  protected virtual void OnDisable()
  {
    vehicle.delistAbilityAction(onAbility);
  }
}
