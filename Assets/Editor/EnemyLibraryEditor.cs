#if UNITY_EDITOR
using static UnityEngine.GraphicsBuffer;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GlobalEnemyLibrary))]
public class EnemyLibraryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GlobalEnemyLibrary library = (GlobalEnemyLibrary)target;

        GUILayout.Space(10);
        if (GUILayout.Button("按敌人等级排序"))
        {
            library.SortByDifficulty();
            EditorUtility.SetDirty(library);
        }

        GUILayout.Label($"当前包含敌人数量: {library.allEnemies.Count}");
    }
}
#endif