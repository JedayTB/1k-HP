using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private CarVisualController _carVisualController;
    private CustomCarPhysics _customCarPhysics;

    void Start()
    {
        _customCarPhysics = GetComponent<CustomCarPhysics>();
        _carVisualController = GetComponent<CarVisualController>();

        _customCarPhysics.Init();
        _carVisualController.Init();
        Debug.Log("Car finished Initialization");
    }

}
