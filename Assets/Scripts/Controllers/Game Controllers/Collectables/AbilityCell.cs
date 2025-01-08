using UnityEngine;

public class AbilityCell : Collectables
{

  [SerializeField] private int _abilityAmount = 100;
  [SerializeField] private BoxCollider _boxCollider;

  [SerializeField] private AbilityCellType _AbilityCellType = AbilityCellType.RandomAbility;



  void Start()
  {
    _boxCollider = GetComponent<BoxCollider>();
    _boxCollider.isTrigger = true;
  }

  private A_Ability GetRandomAbiltiy(A_VehicleController vehicle)
  {
    A_Ability[] abilList = vehicle.GetComponentsInChildren<A_Ability>(true);
    int rndIndex = Random.Range(0, abilList.Length);

    return abilList[rndIndex];
  }

  private void AddAbilityToPlayer(A_VehicleController vehicle)
  {
    A_Ability abilityObj = null;
    switch (_AbilityCellType)
    {
      case AbilityCellType.RandomAbility:
        A_Ability rndAbility = GetRandomAbiltiy(vehicle);
        abilityObj = rndAbility;
        break;
      case AbilityCellType.Bubblegum:
        abilityObj = vehicle.gameObject.GetComponentInChildren<BubblegumController>(true);
        break;
      case AbilityCellType.Hookshot:
        abilityObj = vehicle.gameObject.GetComponentInChildren<HookshotController>(true);
        break;
      case AbilityCellType.Lightning:
        abilityObj = vehicle.gameObject.GetComponentInChildren<LightningController>(true);
        break;
      case AbilityCellType.ChilliOil:
        abilityObj = vehicle.gameObject.GetComponentInChildren<ChilliOilController>(true);
        break;
    }
    abilityObj.vehicle = vehicle;
    abilityObj.gameObject.SetActive(true);
  }

  public override void onPickup(A_VehicleController vehicle)
  {
    AddAbilityToPlayer(vehicle);
    base.onPickup(vehicle);
  }

}


