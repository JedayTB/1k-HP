using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomWheels : MonoBehaviour
{
    #region Variables

    private Rigidbody vehicleRB;
    private bool wheelIsHittingGround;

    [SerializeField] private float wheelSpeed;
    [SerializeField] private bool isFrontWheel;

    
    #endregion

    public void init(Rigidbody rb){
        vehicleRB = rb;
    }
    public void setTireRotation(float yAngle){

    }
    public void applyTireSlide(){

    }
    public void applyAcceleration(){

    }
    public void applyTireSuspensionForces(){

    }

}
