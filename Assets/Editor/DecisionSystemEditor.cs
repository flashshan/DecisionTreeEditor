using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(DecisionSystem))]
[CanEditMultipleObjects]
public class DecisionSystemEditor : Editor
{
    SerializedProperty nodeNameLists_;
    List<string> nodeNames_;
    int searchIndex_;

    void OnEnable()
    {
        nodeNames_ = (serializedObject.targetObject as DecisionSystem).decisions_.nodeNameLists_;

        //nodeNameLists_ = serializedObject.FindProperty("decisions_.nodeNameLists_");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        (target as DecisionSystem).decisions_.curTreeIndex_ = EditorGUILayout.IntField((target as DecisionSystem).decisions_.curTreeIndex_);

        for(int i = 0; i < nodeNames_.Count; ++i)
            EditorGUILayout.LabelField("level" + i + ": " + nodeNames_[i]);

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);

        this.DrawDefaultInspector();
    }
}