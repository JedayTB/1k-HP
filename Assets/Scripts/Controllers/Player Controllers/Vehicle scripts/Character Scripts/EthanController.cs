
using UnityEngine;
using UnityEngine.UI;

public class EthanController : PlayerVehicleController
{
    private int selfColliderID;
    [SerializeField] private Collider selfCollider;
    [SerializeField] private float _maxLightningDistance = 50f;

    [SerializeField] private float cubeSize = 0.5f;
    [SerializeField] private LayerMask VehicleLayer;
    [SerializeField] private Image crosshair;

    
    private I_VehicleController lightningTarget;
    // Update is called once per frame
    public override void Init(InputManager inputManager)
    {
        base.Init(inputManager);
        selfColliderID = selfCollider.GetInstanceID();
        crosshair.enabled = false;
    }
    protected override void Update()
    {
        base.Update();

        if(_canUseAbility)
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

            Ray aimray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(_isDebuging) Debug.DrawRay(aimray.origin, aimray.direction * _maxLightningDistance);

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
            }else
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

            bool hitVehicle = Physics.BoxCast(Camera.main.transform.position, boxSize, lightningDir,
                                             Quaternion.identity, _maxLightningDistance, VehicleLayer);
            if(_isDebuging) Debug.DrawRay(Camera.main.transform.position, lightningDir * _maxLightningDistance);
            if (hitVehicle){
                onVehicleHit();
                _abilityGauge = 0;
            } 
        }
    }
    private void onVehicleHit(){
        print($"Hit {lightningTarget.name}. Respawning");
        lightningTarget.respawn();
    }
}
