using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class A_Ability : MonoBehaviour
{
    public virtual void AbilityUsed(PlayerVehicleController vehicle)
    {
        Debug.Log("Generic ability used");
    }
}
