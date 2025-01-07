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

    private A_Ability AddRandomAbility()
    {
        A_Ability[] abilitiesList = GameStateManager.Instance.Abilitieslist;
        
        int min = 0;
        int max = abilitiesList.Length;

        int rndIndex = Random.Range(0, max);

        return abilitiesList[rndIndex];
    }
       

    public override void onPickup(A_VehicleController vehicle)
    {
        vehicle.addAbilityGauge(_abilityAmount);
        base.onPickup(vehicle);

    }
}

public enum AbilityCellType
{
    RandomAbility,
    Bubblegum,
    Lightning, 
    Hookshot,
}