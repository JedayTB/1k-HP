using System.Collections;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;

public class EthanController : PlayerVehicleController
{

    [Header("Lightning Ability variables")]
    [SerializeField] private Image crosshair;
    [SerializeField] private LineRenderer _lr;
    [SerializeField] private Collider selfCollider;
    [SerializeField] private LayerMask VehicleLayer;
    [SerializeField] private float _maxLightningDistance = 50f;
    [SerializeField] private float cubeSize = 0.5f;
    [SerializeField] private float lightningFadeoutTime = 1.1f;
    [SerializeField] private float timeTillLightningHit = 0.05f;


    private I_VehicleController lightningTarget;
    private int selfColliderID;

    // Update is called once per frame
    public override void Init(InputManager inputManager)
    {
        base.Init(inputManager);
        selfColliderID = selfCollider.GetInstanceID();

        UIController uiCont = FindAnyObjectByType<UIController>();
        crosshair = uiCont.lightningCrossHair;
        crosshair.enabled = false;
    }
    protected override void Update()
    {
        base.Update();

        if (_canUseAbility)
        {
            getAbiliyTarget();
        }
    }
    protected override void onAbilityFull()
    {
        _canUseAbility = true;
        crosshair.enabled = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void getAbiliyTarget()
    {
        Debug.LogWarning("Ability isn't configured to use Input Manager");

        Ray aimray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (_isDebuging) Debug.DrawRay(aimray.origin, aimray.direction * _maxLightningDistance);

        RaycastHit hitInfo;
        Physics.Raycast(aimray.origin, aimray.direction, out hitInfo, _maxLightningDistance, VehicleLayer);

        I_VehicleController vehicleTarget = null;

        if (hitInfo.collider != null && hitInfo.collider.GetInstanceID() != selfColliderID)
        {
            //beautiful code
            //print($"{hitInfo.collider.gameObject.transform.parent.parent.name} id {hitInfo.collider.GetInstanceID()}");
            vehicleTarget = hitInfo.collider.gameObject.transform.parent.parent.gameObject.GetComponent<I_VehicleController>();

        }

        if (vehicleTarget != null) lightningTarget = vehicleTarget;


        if (lightningTarget != null)
        {
            Vector2 UIMove = Camera.main.WorldToScreenPoint(lightningTarget.transform.position);
            crosshair.transform.position = UIMove;
            crosshair.gameObject.SetActive(true);

            float distanceToTarget = Vector3.Distance(transform.position, lightningTarget.transform.position);
            if (distanceToTarget > _maxLightningDistance)
            {
                lightningTarget = null;
                crosshair.gameObject.SetActive(false);
            }
        }
        else
        {
            crosshair.gameObject.SetActive(false);
        }

    }

    public override void useCharacterAbility()
    {
        if (lightningTarget != null && _abilityGauge >= 100)
        {

            Vector3 lightningDir = lightningTarget.transform.position - Camera.main.transform.position;
            Vector3 boxSize = new(cubeSize, cubeSize, cubeSize);

            bool hitVehicle = Physics.BoxCast(Camera.main.transform.position, boxSize, lightningDir, out RaycastHit hit,
                                             Quaternion.identity, _maxLightningDistance, VehicleLayer);
            if (hitVehicle)
            {
                StartCoroutine(fireLightningTo(timeTillLightningHit, hit.point));
                _abilityGauge = 0;

            }
        }
    }
    IEnumerator fireLightningTo(float lightningToDestTime, Vector3 target)
    {
        float count = 0;
        Vector3 lerpedPos = transform.position;

        _lr.positionCount = 2;

        float smoothedProgress = 0;
        while (count < lightningToDestTime)
        {
            count += Time.deltaTime;

            smoothedProgress = count / lightningToDestTime;
            smoothedProgress = LerpAndEasings.ExponentialDecay(smoothedProgress, 1, 7, Time.deltaTime);

            lerpedPos = Vector3.Lerp(transform.position, target, smoothedProgress);

            _lr.SetPosition(0, transform.position);
            _lr.SetPosition(1, lerpedPos);

            yield return null;
        }
        StartCoroutine(fadeoutLightning(lightningFadeoutTime));
        onVehicleHit();
    }
    IEnumerator fadeoutLightning(float lightningFadeoutTime)
    {
        float count = 0f;

        Material lightningMat = _lr.material;
        float opacity = lightningMat.color.a;

        float smoothedProgress;

        Color currentColor = lightningMat.color;
        while (count < lightningFadeoutTime)
        {
            count += Time.deltaTime;

            smoothedProgress = count / lightningFadeoutTime;
            smoothedProgress = LerpAndEasings.ExponentialDecay(smoothedProgress, 1, 7, Time.deltaTime);

            opacity = Mathf.Lerp(opacity, 0f, smoothedProgress);

            currentColor.a = opacity;

            lightningMat.color = currentColor;

            _lr.material = lightningMat;
            yield return null;
        }
        Destroy(lightningMat);
        _lr.positionCount = 0;
    }

    private void onVehicleHit()
    {
        print($"Hit {lightningTarget.name}. Respawning");
        lightningTarget.respawn();
    }
}
