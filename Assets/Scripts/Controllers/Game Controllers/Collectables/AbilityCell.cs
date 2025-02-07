using UnityEngine;

public enum addedAbility
{
  ChilliOil = 0,
  Bubblegum = 1,
  Hookshot = 2,
  Lightning = 3,
  fucked
}
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

  private A_Ability GetRandomAbiltiy(A_VehicleController vehicle, ref AbilityCellType typeAdded)
  {
    A_Ability[] abilList = vehicle.GetComponentsInChildren<A_Ability>(true);
    int rndIndex = Random.Range(0, abilList.Length);

    typeAdded = (AbilityCellType)rndIndex;
    print($"rndAbilFunc: random index {rndIndex}, type casted to {typeAdded}");
    return abilList[rndIndex];
  }

  private void AddAbilityToPlayer(A_VehicleController vehicle)
  {
    A_Ability abilityObj = null;
    AbilityCellType logicEnum = _AbilityCellType;
    addedAbility adAB = addedAbility.fucked;

    if (logicEnum == AbilityCellType.RandomAbility)
    {
      A_Ability rndAbility = GetRandomAbiltiy(vehicle, ref logicEnum);
      abilityObj = rndAbility;
    }

    switch (logicEnum)
    {
      case AbilityCellType.Bubblegum:
        abilityObj = vehicle.gameObject.GetComponentInChildren<BubblegumController>(true);
        adAB = addedAbility.Bubblegum;
        break;
      case AbilityCellType.Hookshot:
        abilityObj = vehicle.gameObject.GetComponentInChildren<HookshotController>(true);
        adAB = addedAbility.Hookshot;
        break;
      case AbilityCellType.Lightning:
        abilityObj = vehicle.gameObject.GetComponentInChildren<LightningController>(true);
        adAB = addedAbility.Lightning;
        break;
      case AbilityCellType.ChilliOil:
        abilityObj = vehicle.gameObject.GetComponentInChildren<ChilliOilController>(true);
        adAB = addedAbility.ChilliOil;
        break;
      default:
        Debug.LogError("Your a dipshit and broke ability add system. to ethan arr (myself)");
        break;
    }
    vehicle.currentAbility = adAB;
    abilityObj.vehicle = vehicle;
    abilityObj.gameObject.SetActive(true);
    if (vehicle is PlayerVehicleController) GameStateManager.Instance._uiController.playerGotAbility(adAB);
    if (vehicle is VehicleAIController) vehicle.GetComponent<VehicleAIController>().addAbilityToVehicle(adAB);
  }

  public override void onPickup(A_VehicleController vehicle)
  {
    AddAbilityToPlayer(vehicle);
    base.onPickup(vehicle);
  }
}


