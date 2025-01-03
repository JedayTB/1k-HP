using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] Transform tpPoint;
    public Transform[] tpTargets;
    Vector3 tpPos;
    void Awake()
    {
        tpPos = tpPoint.position;
    }
    void OnTriggerEnter(Collider other)
    {
        foreach (var t in tpTargets)
        {
            Vector3 n = t.position;
            n.x = tpPos.x;
            t.position = n;
        }
    }
}
