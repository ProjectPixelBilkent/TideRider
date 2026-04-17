using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnergyDebugControls))]
public class EnergyDebugControlsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnergyDebugControls controls = (EnergyDebugControls)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("Fill Energy To Max"))
        {
            controls.FillEnergyToMax();
            EditorUtility.SetDirty(controls);
        }

        if (GUILayout.Button("Apply Custom Energy"))
        {
            controls.ApplyCustomEnergy();
            EditorUtility.SetDirty(controls);
        }
    }
}
