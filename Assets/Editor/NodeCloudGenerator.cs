using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NodeCloudUtil))]
public class NodeCloudGenerator : Editor
{
  NodeCloudUtil NCU;


  private void Awake()
  {
    NCU = (NodeCloudUtil)target;
  }
  // Update is called once per frame
  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    if (GUILayout.Button("Create Node Cloud"))
    {
      NCU.CreateNodeCloud();
    }
    if (GUILayout.Button("Delete Node Cloud"))
    {
      NCU.DeleteNodeCloud();
    }
  }
}
