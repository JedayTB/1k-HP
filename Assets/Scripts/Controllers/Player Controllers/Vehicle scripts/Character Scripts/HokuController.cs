using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HokuController : PlayerVehicleController
{
    [SerializeField] private float _maxHookDistance = 100f;
    [SerializeField] private Image _hookCrossHair;
    [SerializeField] private LayerMask _HookShottableLayers;
    [SerializeField] private Collider _selfCollider;

    private int _selfColliderID;
    Vector3 _hookShotPos;

    public override void Init(InputManager inp)
    {

        base.Init(inp);
        //_hookCrossHair.enabled = false;
        _selfColliderID = _selfCollider.GetInstanceID();

    }

    protected override void onAbilityFull()
    {
        _canUseAbility = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


    }


    protected override void Update()
    {
        base.Update();

        if (_canUseAbility)
        {
            getHookShotTarget();
        }
    }

    private void getHookShotTarget()
    {
        Ray aimray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (_isDebuging) Debug.DrawRay(aimray.origin, aimray.direction * _maxHookDistance);

        Physics.Raycast(aimray.origin, aimray.direction, out RaycastHit hitInfo, _maxHookDistance, _HookShottableLayers);

        if (hitInfo.collider != null && hitInfo.collider.GetInstanceID() != _selfColliderID && Input.GetMouseButtonDown(0))
        {
            Debug.LogWarning("Ability isn't configured to use Input Manager");
            _hookCrossHair.enabled = true;
            _hookShotPos = hitInfo.point;

            //Check if ray hit a vehicle
            //vehicleTarget = hitInfo.collider.gameObject.transform.parent.parent.gameObject.GetComponent<I_VehicleController>();
        }
        Vector2 UIMove = Camera.main.WorldToScreenPoint(_hookShotPos);
        if (_hookCrossHair.enabled) _hookCrossHair.transform.position = UIMove;
    }

    public override void useCharacterAbility()
    {
        StartCoroutine(hookShotToTarget(3f, 0f,_hookShotPos));
    }
    IEnumerator hookShotToTarget(float timeToDestination, float centerOffset,Vector3 destination)
    {
        
        float secondCount = 0f;
        Vector3 startPos = transform.position;
        Vector3 slerpPos;

        //Fix Slerp when not at 0,0
        Vector3 centerPivot = (startPos + destination) * 0.5f;
        centerPivot -= new Vector3(0, -centerOffset);
        Vector3 startRelativeCenter = startPos - centerPivot;
        Vector3 endRelativeCenter = destination - centerPivot;
        

        float progress;

        while (secondCount < timeToDestination)
        {
            secondCount += Time.deltaTime;
            progress = secondCount / timeToDestination;
            slerpPos = Vector3.Slerp(startRelativeCenter, endRelativeCenter ,progress ) + centerPivot;

            transform.position = slerpPos;
            Debug.DrawRay(transform.position, slerpPos, Color.blue);

            yield return null;
        }
        _hookCrossHair.enabled = false;
    }

}
