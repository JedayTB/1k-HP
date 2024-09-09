using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(easyBuildingBuilder))]
public class easyBuildingGenerator : Editor
{
    easyBuildingBuilder builder;
    private void Awake()
    {
        builder = (easyBuildingBuilder)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Create Building"))
        {
            builder.createBuilding();
        }
    }
}
