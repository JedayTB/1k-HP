using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class HokuController : PlayerVehicleController
{
    [SerializeField] private float _maxHookDistance = 50f;
    [SerializeField] private Image _hookCrossHair;
    [SerializeField] private LayerMask _HookShottableLayers;
    [SerializeField] private Collider _selfCollider;

    private int _selfColliderID;

    Vector3 _hookShotPos;

    public override void Init(InputManager inp)
    {

        base.Init(inp);
        _hookCrossHair.enabled = false;
        _selfColliderID = _selfCollider.GetInstanceID();

    }

    protected override void onAbilityFull()
    {
        _canUseAbility = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _hookCrossHair.enabled = true;
    }


    protected override void Update()
    {
        base.Update();

        if (_canUseAbility) { 
            
        }
    }

    private void getHookShotTarget()
    {
        Ray aimray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (_isDebuging) Debug.DrawRay(aimray.origin, aimray.direction * _maxHookDistance);

        RaycastHit hitInfo;
        Physics.Raycast(aimray.origin, aimray.direction, out hitInfo, _maxHookDistance, _HookShottableLayers);

        

        if (hitInfo.collider != null && hitInfo.collider.GetInstanceID() != _selfColliderID)
        {
            _hookShotPos = hitInfo.point;
            //Check if ray hit a vehicle
            //vehicleTarget = hitInfo.collider.gameObject.transform.parent.parent.gameObject.GetComponent<I_VehicleController>();
        }



    }

    public override void useCharacterAbility()
    {

    }

}
