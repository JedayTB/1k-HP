using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeCloudRetriever : MonoBehaviour
{
    public List<NodePoint> CurrentNodes;
    void Awake()
    {
        CurrentNodes = new();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<NodePoint>(out var nodepoint))
        {
            CurrentNodes.Add(nodepoint);
        }

    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<NodePoint>(out var nodepoint))
        {
            CurrentNodes.Remove(nodepoint);
        }
    }
}
