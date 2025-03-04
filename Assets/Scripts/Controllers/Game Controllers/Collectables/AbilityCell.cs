using UnityEngine;
using System.Collections;

public enum addedAbility
{
  Bubblegum = 0,
  Lightning = 1,
  fucked
}

public class AbilityCell : Collectables
{
  private static readonly int dissolveDistanceID = Shader.PropertyToID("_dissolveDistance");
  private static readonly int dissolveHeightID = Shader.PropertyToID("_dissolveHeight");

  private static readonly float minDissolveHeight = 0f;
  private static readonly float maxDissolveHeight = 1f;


  private static readonly float minDissolveDistance = 0f;
  private static readonly float maxDissolveDistance = 1f;

  [SerializeField] private int _abilityAmount = 100;
  [SerializeField] private BoxCollider _boxCollider;
  [SerializeField] private AbilityCellType _AbilityCellType = AbilityCellType.RandomAbility;

  private Material abilMat;

  void Start()
  {
    _boxCollider = GetComponent<BoxCollider>();
    _boxCollider.isTrigger = true;
    // because I remember jordan telling me this makes a copy.
    Destroy(abilMat);
    abilMat = _renderer.material;
  }

  protected override IEnumerator respawnClock()
  {
    float count = 0f;

    float progess = 0f;

    float dissolveHeight;
    //float dissolveDistance;
    _collider.enabled = false;
    while (count < _timeToRespawn)
    {
      count += Time.deltaTime;

      progess = count / _timeToRespawn;

      dissolveHeight = Mathf.Lerp(minDissolveHeight, maxDissolveHeight, progess);

      abilMat.SetFloat(dissolveHeightID, dissolveHeight);
      //dissolveDistance = Mathf.Lerp(minDissolveDistance, maxDissolveDistance, progess);
      //abilMat.SetFloat(dissolveDistanceID, dissolveDistance);

      yield return null;
    }
    _collider.enabled = true;
    _renderer.enabled = true;
  }

  private A_Ability GetRandomAbiltiy(A_VehicleController vehicle, ref AbilityCellType typeAdded)
  {
    var abilList = GameStateManager.Instance.Abilitieslist;
    float rnd = Random.Range(0, 1f);
    int index = 0;

    if (rnd > 0.5f)
    {
      // Bubble gum
      index = (int)AbilityCellType.Bubblegum;
    }
    else
    {
      // Lightning
      index = (int)AbilityCellType.Lightning;
    }
    typeAdded = (AbilityCellType)index;

    print($"rndAbilFunc: random index {index}, type casted to {typeAdded}");
    return abilList[index];
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
      case AbilityCellType.Lightning:
        abilityObj = vehicle.gameObject.GetComponentInChildren<LightningController>(true);
        adAB = addedAbility.Lightning;
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


