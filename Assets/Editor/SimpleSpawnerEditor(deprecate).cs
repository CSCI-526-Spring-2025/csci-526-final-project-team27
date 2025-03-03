//#if UNITY_EDITOR
//using UnityEditor;
//using UnityEngine;
//using System.Collections.Generic;

//[CustomEditor(typeof(SimpletSpawner))]
//public class SimpleSpawnerEditor : Editor
//{
//    private SerializedProperty dynamicSpawnRangeMin;
//    private SerializedProperty dynamicSpawnRangeMax;
//    private SerializedProperty difficultyCurve;
//    private SerializedProperty dynamicSpawnPosition;
//    private SerializedProperty waveGroups;
//    private SerializedProperty enemyLibrary;
//    private SerializedProperty spawnIntervalMin;
//    private SerializedProperty spawnIntervalMax;

//    private bool showWaveSettings = true;

//    // 可视化编辑状态
//    private int selectedWaveIndex = -1;
//    private int selectedUnitIndex = -1;
//    private bool isPlacingPoints = false;
//    private SimpletSpawner.SpawnPosition currentPositionType;

//    private Dictionary<int, bool> waveFoldoutStates = new Dictionary<int, bool>();

//    private void OnEnable()
//    {
//        dynamicSpawnRangeMin = serializedObject.FindProperty("dynamicSpawnRangeMin");
//        dynamicSpawnRangeMax = serializedObject.FindProperty("dynamicSpawnRangeMax");
//        difficultyCurve = serializedObject.FindProperty("difficultyCurve");
//        dynamicSpawnPosition = serializedObject.FindProperty("dynamicSpawnPosition");
//        waveGroups = serializedObject.FindProperty("waveGroups");
//        enemyLibrary = serializedObject.FindProperty("enemyLibrary");
//        spawnIntervalMin = serializedObject.FindProperty("spawnIntervalMin");
//        spawnIntervalMax = serializedObject.FindProperty("spawnIntervalMax");

//        SceneView.duringSceneGui += OnSceneGUI;
//    }

//    private void OnDisable()
//    {
//        SceneView.duringSceneGui -= OnSceneGUI;
//    }

//    private void OnSceneGUI(SceneView sceneView)
//    {
//        SimpletSpawner spawner = target as SimpletSpawner;
//        if (spawner == null || spawner.roomPrefab == null)
//            return;

//        // 检查波次数组是否为空以及索引是否有效
//        if (waveGroups.arraySize == 0 || selectedWaveIndex < 0 || selectedWaveIndex >= waveGroups.arraySize)
//            return;

//        SerializedProperty wave = waveGroups.GetArrayElementAtIndex(selectedWaveIndex);
//        SerializedProperty units = wave.FindPropertyRelative("units");
//        if (units.arraySize == 0 || selectedUnitIndex < 0 || selectedUnitIndex >= units.arraySize)
//            return;

//        SerializedProperty unit = units.GetArrayElementAtIndex(selectedUnitIndex);
//        SerializedProperty positionType = unit.FindPropertyRelative("positionType");
//        SerializedProperty spawnPositions = unit.FindPropertyRelative("spawnPositions");

//        currentPositionType = (SimpletSpawner.SpawnPosition)positionType.enumValueIndex;
//        if (currentPositionType != SimpletSpawner.SpawnPosition.FixedPoints)
//            return;

//        // 仅在 Repaint 或鼠标拖拽事件时绘制句柄
//        if (Event.current.type == EventType.Repaint || Event.current.type == EventType.MouseDrag)
//        {
//            DrawPositionHandles(spawner, spawnPositions);
//        }

//        ProcessSceneEvents(spawner, spawnPositions);
//    }

//    private void DrawPositionHandles(SimpletSpawner spawner, SerializedProperty spawnPositions)
//    {
//        if (spawner.roomPrefab == null) return;
//        Transform roomTransform = spawner.roomPrefab.transform;

//        for (int i = 0; i < spawnPositions.arraySize; i++)
//        {
//            SerializedProperty posProp = spawnPositions.GetArrayElementAtIndex(i);
//            Vector2 localPos = posProp.vector2Value;
//            Vector3 worldPos = roomTransform.TransformPoint(localPos);

//            EditorGUI.BeginChangeCheck();
//            Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);
//            if (EditorGUI.EndChangeCheck())
//            {
//                Undo.RecordObject(spawner, "Move Spawn Point");
//                posProp.vector2Value = roomTransform.InverseTransformPoint(newWorldPos);
//                serializedObject.ApplyModifiedProperties();
//            }
//            Handles.Label(worldPos, $"Point {i + 1}");
//        }
//    }

//    private void ProcessSceneEvents(SimpletSpawner spawner, SerializedProperty spawnPositions)
//    {
//        if (spawner.roomPrefab == null) return;
//        Transform roomTransform = spawner.roomPrefab.transform;

//        Event e = Event.current;
//        if (e.type != EventType.MouseDown || e.button != 0 || !isPlacingPoints)
//            return;

//        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
//        if (Physics.Raycast(ray, out RaycastHit hit))
//        {
//            Undo.RecordObject(spawner, "Add Spawn Point");

//            int newIndex = spawnPositions.arraySize;
//            spawnPositions.arraySize = newIndex + 1;
//            SerializedProperty newPos = spawnPositions.GetArrayElementAtIndex(newIndex);

//            Vector3 localPos = roomTransform.InverseTransformPoint(hit.point);
//            newPos.vector2Value = new Vector2(localPos.x, localPos.y);

//            serializedObject.ApplyModifiedProperties();
//            e.Use();
//        }
//    }

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();

//        EditorGUILayout.Space();
//        EditorGUILayout.LabelField("房间基础配置", EditorStyles.boldLabel);
//        EditorGUILayout.PropertyField(serializedObject.FindProperty("roomPrefab"));
//        EditorGUILayout.PropertyField(serializedObject.FindProperty("roomType"));
//        EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnMode"));

//        EditorGUILayout.PropertyField(enemyLibrary);
//        EditorGUILayout.PropertyField(spawnIntervalMin);
//        EditorGUILayout.PropertyField(spawnIntervalMax);


//        if (serializedObject.FindProperty("spawnMode").enumValueIndex ==
//            (int)SimpletSpawner.SpawnMode.Dynamic)
//        {
//            DrawDynamicSettings();
//        }
//        else
//        {
//            DrawStaticWaveSettings();
//        }

//        serializedObject.ApplyModifiedProperties();
//    }

//    private void DrawDynamicSettings()
//    {
//        EditorGUILayout.Space();
//        EditorGUILayout.LabelField("动态生成设置", EditorStyles.boldLabel);
//        EditorGUILayout.PropertyField(dynamicSpawnRangeMin);
//        EditorGUILayout.PropertyField(dynamicSpawnRangeMax);
//        EditorGUILayout.PropertyField(difficultyCurve);
//        EditorGUILayout.PropertyField(dynamicSpawnPosition);
//    }

//    private void DrawStaticWaveSettings()
//    {
//        EditorGUILayout.Space();
//        showWaveSettings = EditorGUILayout.Foldout(showWaveSettings, "静态波次配置", true);
//        if (!showWaveSettings) return;

//        EditorGUI.indentLevel++;

//        for (int i = 0; i < waveGroups.arraySize; i++)
//        {
//            DrawWaveGroup(i);
//        }

//        EditorGUILayout.BeginHorizontal();
//        if (GUILayout.Button("添加波次"))
//        {
//            waveGroups.arraySize++;
//        }
//        if (GUILayout.Button("清空全部波次"))
//        {
//            waveGroups.ClearArray();
//        }
//        EditorGUILayout.EndHorizontal();

//        EditorGUI.indentLevel--;
//    }

//    private void DrawWaveGroup(int waveIndex)
//    {
//        SerializedProperty wave = waveGroups.GetArrayElementAtIndex(waveIndex);
//        SerializedProperty waveName = wave.FindPropertyRelative("waveName");
//        SerializedProperty units = wave.FindPropertyRelative("units");

//        EditorGUILayout.BeginVertical("box");

//        // 使用字典维护每个波次的折叠状态
//        if (!waveFoldoutStates.ContainsKey(waveIndex))
//            waveFoldoutStates[waveIndex] = true;
//        bool isExpanded = waveFoldoutStates[waveIndex];

//        EditorGUILayout.BeginHorizontal();
//        isExpanded = EditorGUILayout.Foldout(isExpanded, $"波次 {waveIndex + 1}: {waveName.stringValue}", true);
//        waveFoldoutStates[waveIndex] = isExpanded;
//        if (GUILayout.Button("×", GUILayout.Width(20)))
//        {
//            waveGroups.DeleteArrayElementAtIndex(waveIndex);
//            return;
//        }
//        EditorGUILayout.EndHorizontal();

//        if (isExpanded)
//        {
//            EditorGUILayout.PropertyField(waveName);
//            EditorGUILayout.PropertyField(wave.FindPropertyRelative("preDelay"));

//            EditorGUILayout.Space();
//            EditorGUILayout.LabelField("敌人生成单元", EditorStyles.boldLabel);
//            for (int i = 0; i < units.arraySize; i++)
//            {
//                DrawSpawnUnit(waveIndex, i, units.GetArrayElementAtIndex(i));
//            }

//            EditorGUILayout.BeginHorizontal();
//            if (GUILayout.Button("添加生成单元"))
//            {
//                units.arraySize++;
//            }
//            if (GUILayout.Button("清空单元"))
//            {
//                units.ClearArray();
//            }
//            EditorGUILayout.EndHorizontal();
//        }

//        EditorGUILayout.EndVertical();
//    }

//    private void DrawSpawnUnit(int waveIndex, int unitIndex, SerializedProperty unit)
//    {
//        EditorGUILayout.BeginVertical("box");
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField($"生成单元 {unitIndex + 1}", EditorStyles.boldLabel);
//        if (GUILayout.Button("×", GUILayout.Width(20)))
//        {
//            SerializedProperty wave = waveGroups.GetArrayElementAtIndex(waveIndex);
//            SerializedProperty units = wave.FindPropertyRelative("units");
//            units.DeleteArrayElementAtIndex(unitIndex);
//            return;
//        }
//        EditorGUILayout.EndHorizontal();

//        EditorGUILayout.PropertyField(unit.FindPropertyRelative("enemy"));
//        EditorGUILayout.PropertyField(unit.FindPropertyRelative("count"));
//        EditorGUILayout.PropertyField(unit.FindPropertyRelative("interval"));

//        SerializedProperty positionType = unit.FindPropertyRelative("positionType");
//        EditorGUILayout.PropertyField(positionType);

//        // FixedPoints模式下处理预设位置编辑
//        if ((SimpletSpawner.SpawnPosition)positionType.enumValueIndex == SimpletSpawner.SpawnPosition.FixedPoints)
//        {
//            DrawFixedPointsSettings(waveIndex, unitIndex, unit);
//        }
//        EditorGUILayout.EndVertical();
//    }

//    private void DrawFixedPointsSettings(int waveIndex, int unitIndex, SerializedProperty unit)
//    {
//        SerializedProperty spawnPositions = unit.FindPropertyRelative("spawnPositions");
//        EditorGUILayout.BeginVertical("helpbox");

//        // 编辑模式开关：仅在状态切换时调用一次 RepaintAll
//        bool wasPlacing = isPlacingPoints;
//        isPlacingPoints = GUILayout.Toggle(isPlacingPoints, "场景编辑模式", "Button", GUILayout.Height(25));
//        if (isPlacingPoints && !wasPlacing)
//        {
//            selectedWaveIndex = waveIndex;
//            selectedUnitIndex = unitIndex;
//            SceneView.RepaintAll();
//        }

//        EditorGUILayout.LabelField("预设位置列表（相对房间中心）：");
//        for (int i = 0; i < spawnPositions.arraySize; i++)
//        {
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.PropertyField(spawnPositions.GetArrayElementAtIndex(i), GUIContent.none);
//            if (GUILayout.Button("定位", GUILayout.Width(50)))
//            {
//                Vector3 worldPos = ((SimpletSpawner)target).roomPrefab.transform.TransformPoint(
//                    spawnPositions.GetArrayElementAtIndex(i).vector2Value
//                );
//                SceneView.lastActiveSceneView.pivot = worldPos;
//                SceneView.lastActiveSceneView.Repaint();
//            }
//            if (GUILayout.Button("×", GUILayout.Width(20)))
//            {
//                spawnPositions.DeleteArrayElementAtIndex(i);
//            }
//            EditorGUILayout.EndHorizontal();
//        }

//        EditorGUILayout.BeginHorizontal();
//        if (GUILayout.Button("添加位置"))
//        {
//            Undo.RecordObject(target, "Add Spawn Point");
//            int newIndex = spawnPositions.arraySize;
//            spawnPositions.arraySize = newIndex + 1;
//            serializedObject.ApplyModifiedProperties();
//        }
//        if (GUILayout.Button("清除全部"))
//        {
//            Undo.RecordObject(target, "Clear Spawn Points");
//            spawnPositions.ClearArray();
//            serializedObject.ApplyModifiedProperties();
//        }
//        EditorGUILayout.EndHorizontal();

//        EditorGUILayout.EndVertical();
//    }
//}
//#endif