using System.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class HokuController : PlayerVehicleController
{
    [Header("Ability Basics")]
    [SerializeField] private float _maxHookDistance = 100f;
    [SerializeField] private Image _hookCrossHair;
    [SerializeField] private LayerMask _HookShottableLayers;
    [SerializeField] private Collider _selfCollider;

    [Header("Hook shot Specifics")]

    [SerializeField] LineRenderer _lineRenderer;
    [SerializeField] Transform hookshotOrigin, _camera;
    [SerializeField] SpringJoint _springJoint;

    [SerializeField] private float _maxSpringDistanceMultiplier = 0.8f;
    [SerializeField] private float _minSpringDistanceMultiplier = 0.25f;

    [SerializeField] private float _springForce = 4.5f;
    [SerializeField] private float _springDamper = 7f;
    [SerializeField] private float _massScale = 4.5f;

    bool hookshotPositionLocked = false;
    private int _selfColliderID;
    Vector3 _hookShotPos;

    public override void Init(InputManager inp)
    {

        base.Init(inp);
        //_hookCrossHair.enabled = false;
        if (_selfCollider == null)
        {
            Debug.LogError("Set hoku's main Collider in Inspector!");

        }
        else
        {
            _selfColliderID = _selfCollider.GetInstanceID();
        }

        UIController uiCont = FindAnyObjectByType<UIController>();
        _hookCrossHair = uiCont.HookshotCrosshair;

    }

    protected override void onAbilityFull()
    {
        print("HELLO?");
        _canUseAbility = true;
        _hookCrossHair.enabled = true;

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
    private void LateUpdate()
    {
        DrawRope();
    }
    private void DrawRope()
    {
        // If not grappling, don't draw rope
        if (!_springJoint)
        {
            //lerp positions somewhere in here
            _lineRenderer.positionCount = 2;

            _lineRenderer.SetPosition(0, transform.position);
            _lineRenderer.SetPosition(1, _hookShotPos);
        }
        else
        {
            _lineRenderer.positionCount = 0;
        }
    }
    private void InitializeSpringJoint(Vector3 springJointPosition)
    {
        _springJoint = this.gameObject.AddComponent<SpringJoint>();
        _springJoint.autoConfigureConnectedAnchor = false;
        _springJoint.connectedAnchor = springJointPosition;

        float distanceFromPoint = Vector3.Distance(transform.position, _hookShotPos);

        //Configure later
        _springJoint.maxDistance = distanceFromPoint * 0.8f;
        _springJoint.minDistance = distanceFromPoint * 0.25f;

        _springJoint.spring = 4.5f;
        _springJoint.damper = 7f;
        _springJoint.massScale = 4.5f;

    }
    private void InitializeLineRenderer()
    {
        _lineRenderer.positionCount = 2;
    }

    private void getHookShotTarget()
    {
        Ray aimray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (_isDebuging) Debug.DrawRay(aimray.origin, aimray.direction * _maxHookDistance);

        Physics.Raycast(aimray.origin, aimray.direction, out RaycastHit hitInfo, _maxHookDistance, _HookShottableLayers);

        //Shoots a ray
        if (hitInfo.collider != null && hitInfo.collider.GetInstanceID() != _selfColliderID && Input.GetMouseButtonDown(0))
        {
            Debug.LogWarning("Ability isn't configured to use Input Manager");
            _hookCrossHair.enabled = true;
            _hookShotPos = hitInfo.point;
            hookshotPositionLocked = true;
        }
        else if (hookshotPositionLocked == false)
        {
            //Because 0,0,0 is a valid hookshot position
            _hookShotPos = transform.position;
        }
        Vector2 UIMove = Camera.main.WorldToScreenPoint(_hookShotPos);
        if (_hookCrossHair.enabled) _hookCrossHair.transform.position = UIMove;
    }


    public override void useCharacterAbility()
    {
        if (_hookShotPos != transform.position)
        {
            InitializeSpringJoint(_hookShotPos);
            InitializeLineRenderer();
        }
    }
    IEnumerator hookShotToTarget(float timeToDestination, float centerOffset, Vector3 destination)
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
            slerpPos = Vector3.Slerp(startRelativeCenter, endRelativeCenter, progress) + centerPivot;

            transform.position = slerpPos;
            Debug.DrawRay(transform.position, slerpPos, Color.blue);

            yield return null;
        }
        _hookCrossHair.enabled = false;
    }

}