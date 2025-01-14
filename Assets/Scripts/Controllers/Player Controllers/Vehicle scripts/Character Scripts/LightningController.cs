using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LightningController : A_Ability
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

    private bool _canUseAbility = false;
    private A_VehicleController lightningTarget;
    private int selfColliderID;

    // InputManager inputManager
    // Update is called once per frame
    void Awake()
    {
        selfColliderID = selfCollider.GetInstanceID();
        UIController uiCont = FindAnyObjectByType<UIController>();
        crosshair = uiCont.lightningCrossHair;
        crosshair.gameObject.SetActive(false);
        

        onAbility = AbilityUsed;
    }
    void Update()
    {
        if (_canUseAbility)
        {
            getAbiliyTarget();
        }
    }
    private void startAbility()
    {
        _canUseAbility = true;
        crosshair.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        print("Start looking for target");
    }

    private void getAbiliyTarget()
    {
        //Debug.LogWarning("Ability isn't configured to use Input Manager");

        Ray aimray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (GameStateManager.Instance.UseDebug) Debug.DrawRay(aimray.origin, aimray.direction * _maxLightningDistance);

        RaycastHit hitInfo;
        Physics.Raycast(aimray.origin, aimray.direction, out hitInfo, _maxLightningDistance, VehicleLayer);

        A_VehicleController vehicleTarget = null;

        if (hitInfo.collider != null && hitInfo.collider.GetInstanceID() != selfColliderID)
        {
            //beautiful code
            //print($"{hitInfo.collider.gameObject.transform.parent.parent.name} id {hitInfo.collider.GetInstanceID()}");
            vehicleTarget = hitInfo.collider.gameObject.transform.parent.parent.gameObject.GetComponent<A_VehicleController>();

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

    public override void AbilityUsed()
    {   
        if(_canUseAbility == false)
        {
            startAbility();
        }
        else if(_canUseAbility == true) 
        {
            strikeLightning();
        }  
    }
    private void strikeLightning()
    {
        if (lightningTarget != null)
        {

            Vector3 lightningDir = lightningTarget.transform.position - Camera.main.transform.position;
            Vector3 boxSize = new(cubeSize, cubeSize, cubeSize);

            bool hitVehicle = Physics.BoxCast(Camera.main.transform.position, boxSize, lightningDir, out RaycastHit hit,
                                             Quaternion.identity, _maxLightningDistance, VehicleLayer);
            if (hitVehicle)
            {
                StartCoroutine(fireLightningTo(timeTillLightningHit, hit.point));
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
    /// <summary>
    /// Last function called before disabled
    /// </summary>
    /// <param name="lightningFadeoutTime"></param>
    /// <returns></returns>
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
        gameObject.SetActive(false);
    }
    
    private void onVehicleHit()
    {
        print($"Hit {lightningTarget.name}. Respawning");
        lightningTarget.respawn();
    }
    
}
