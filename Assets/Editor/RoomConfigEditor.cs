#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomConfig))]
public class RoomConfigEditor : Editor
{
    private SerializedProperty dynamicSpawnRange;
    private SerializedProperty waveGroups;
    private bool showWaveSettings = true;

    private void OnEnable()
    {
        dynamicSpawnRange = serializedObject.FindProperty("dynamicSpawnRange");
        waveGroups = serializedObject.FindProperty("waveGroups");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 基础设置
        EditorGUILayout.PropertyField(serializedObject.FindProperty("roomType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnMode"));

        // 动态生成设置
        if (serializedObject.FindProperty("spawnMode").enumValueIndex ==
            (int)RoomConfig.SpawnMode.Dynamic)
        {
            EditorGUILayout.PropertyField(dynamicSpawnRange);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("difficultyCurve"));
        }
        // 静态波次设置
        else
        {
            showWaveSettings = EditorGUILayout.Foldout(showWaveSettings, "波次配置");
            if (showWaveSettings)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < waveGroups.arraySize; i++)
                {
                    DrawWaveGroup(waveGroups.GetArrayElementAtIndex(i), i);
                }
                WaveManagementButtons();
                EditorGUI.indentLevel--;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawWaveGroup(SerializedProperty wave, int index)
    {
        EditorGUILayout.BeginVertical("box");

        // 波次标题
        EditorGUILayout.BeginHorizontal();
        wave.isExpanded = EditorGUILayout.Foldout(
            wave.isExpanded,
            $"波次 {index + 1}: {wave.FindPropertyRelative("waveName").stringValue}",
            true
        );
        if (GUILayout.Button("×", GUILayout.Width(20)))
        {
            waveGroups.DeleteArrayElementAtIndex(index);
            return;
        }
        EditorGUILayout.EndHorizontal();

        if (wave.isExpanded)
        {
            // 基本参数
            EditorGUILayout.PropertyField(wave.FindPropertyRelative("waveName"));
            EditorGUILayout.PropertyField(wave.FindPropertyRelative("preDelay"));
            EditorGUILayout.PropertyField(wave.FindPropertyRelative("postDelay"));

            // 生成单元
            SerializedProperty units = wave.FindPropertyRelative("units");
            EditorGUILayout.PropertyField(units, true);
        }

        EditorGUILayout.EndVertical();
    }

    private void WaveManagementButtons()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("添加波次"))
        {
            waveGroups.arraySize++;
        }
        if (GUILayout.Button("清空全部"))
        {
            waveGroups.ClearArray();
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif