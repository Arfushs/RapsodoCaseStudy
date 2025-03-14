using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

public class GameObjectManager : EditorWindow
{
    private List<GameObject> _gameObjects = new List<GameObject>();
    private Vector2 _scrollPos;
    private int _lastObjectCount;
    private List<GameObject> _selectedObjects = new List<GameObject>();

    // Editing Tranform variables
    private Vector3 _position;
    private Vector3 _rotation;
    private Vector3 _scale;
    private bool[] _positionMixed = new bool[3];
    private bool[] _rotationMixed = new bool[3];
    private bool[] _scaleMixed = new bool[3];

    // Filtering options
    private bool _filterMeshRenderer;
    private bool _filterCollider;
    private bool _filterRigidbody;
    private string _searchQuery = "";

    [MenuItem("Tools/GameObject Manager")]
    public static void ShowWindow()
    {
        GetWindow<GameObjectManager>("GameObject Manager");
    }

    private void OnEnable()
    {
        RefreshGameObjects();
        EditorApplication.update += AutoRefresh;
    }

    private void OnDisable()
    {
        EditorApplication.update -= AutoRefresh;
    }

    private void OnGUI()
    {
        DrawSearchOptions();
        DrawFilteringOptions();
        DrawGameObjectList();
        EditorGUILayout.Space();

        if (_selectedObjects.Count > 0)
        {
            DrawTransformEditor();
            EditorGUILayout.Space();
            DrawComponentEditor();
        }
    }

    private void DrawFilteringOptions()
    {
        EditorGUILayout.LabelField("Filtering Options", EditorStyles.boldLabel);
        _filterMeshRenderer = EditorGUILayout.Toggle("Mesh Renderer", _filterMeshRenderer);
        _filterCollider = EditorGUILayout.Toggle("Collider", _filterCollider);
        _filterRigidbody = EditorGUILayout.Toggle("Rigidbody", _filterRigidbody);
        EditorGUILayout.Space();
    }

    private void DrawSearchOptions()
    {
        EditorGUILayout.LabelField("Search Options", EditorStyles.boldLabel);
        _searchQuery = EditorGUILayout.TextField("Search", _searchQuery);
    }

    private void DrawGameObjectList()
    {
        EditorGUILayout.LabelField("GameObject List", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        foreach (var obj in _gameObjects)
        {
            // Filtering: If the corresponding filter is active and the object does not have the required component, skip.
            if (_filterMeshRenderer && obj.GetComponent<MeshRenderer>() == null)
                continue;
            if (_filterCollider && obj.GetComponent<Collider>() == null)
                continue;
            if (_filterRigidbody && obj.GetComponent<Rigidbody>() == null)
                continue;
            // Search filter: If the search query is not empty and the object name does not contain it, skip.
            if (!string.IsNullOrEmpty(_searchQuery) &&
                obj.name.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            if (obj != null)
            {
                EditorGUILayout.BeginHorizontal("box");
                // State information
                if (!obj.activeSelf)
                    EditorGUILayout.LabelField("Disabled", EditorStyles.boldLabel, GUILayout.Width(120));
                else
                    EditorGUILayout.LabelField("Enabled", EditorStyles.boldLabel, GUILayout.Width(120));

                bool newState = EditorGUILayout.Toggle(obj.activeSelf, GUILayout.Width(20));
                if (newState != obj.activeSelf)
                {
                    Undo.RecordObject(obj, "Toggle Active State");
                    obj.SetActive(newState);
                }

                // Selection handling
                bool isSelected = _selectedObjects.Contains(obj);
                if (GUILayout.Toggle(isSelected, obj.name, "Button", GUILayout.Width(200)))
                {
                    if (!isSelected)
                        _selectedObjects.Add(obj);
                }
                else
                {
                    _selectedObjects.Remove(obj);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawTransformEditor()
    {
        EditorGUILayout.LabelField("Edit Selected GameObjects", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        DetermineCommonTransformValues();

        // Position
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Position", GUILayout.Width(70));
        float oldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 15;

        EditorGUI.showMixedValue = _positionMixed[0];
        float newPosX = EditorGUILayout.FloatField("X", _position.x, GUILayout.Width(60));
        EditorGUI.showMixedValue = false;

        EditorGUI.showMixedValue = _positionMixed[1];
        float newPosY = EditorGUILayout.FloatField("Y", _position.y, GUILayout.Width(60));
        EditorGUI.showMixedValue = false;

        EditorGUI.showMixedValue = _positionMixed[2];
        float newPosZ = EditorGUILayout.FloatField("Z", _position.z, GUILayout.Width(60));
        EditorGUI.showMixedValue = false;

        EditorGUIUtility.labelWidth = oldLabelWidth;
        EditorGUILayout.EndHorizontal();
        Vector3 newPosition = new Vector3(newPosX, newPosY, newPosZ);

        // Rotation
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Rotation", GUILayout.Width(70));
        oldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 15;

        EditorGUI.showMixedValue = _rotationMixed[0];
        float newRotX = EditorGUILayout.FloatField("X", _rotation.x, GUILayout.Width(60));
        EditorGUI.showMixedValue = false;

        EditorGUI.showMixedValue = _rotationMixed[1];
        float newRotY = EditorGUILayout.FloatField("Y", _rotation.y, GUILayout.Width(60));
        EditorGUI.showMixedValue = false;

        EditorGUI.showMixedValue = _rotationMixed[2];
        float newRotZ = EditorGUILayout.FloatField("Z", _rotation.z, GUILayout.Width(60));
        EditorGUI.showMixedValue = false;

        EditorGUIUtility.labelWidth = oldLabelWidth;
        EditorGUILayout.EndHorizontal();
        Vector3 newRotation = new Vector3(newRotX, newRotY, newRotZ);

        // Scale
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Scale", GUILayout.Width(70));
        oldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 15;

        EditorGUI.showMixedValue = _scaleMixed[0];
        float newScaleX = EditorGUILayout.FloatField("X", _scale.x, GUILayout.Width(60));
        EditorGUI.showMixedValue = false;

        EditorGUI.showMixedValue = _scaleMixed[1];
        float newScaleY = EditorGUILayout.FloatField("Y", _scale.y, GUILayout.Width(60));
        EditorGUI.showMixedValue = false;

        EditorGUI.showMixedValue = _scaleMixed[2];
        float newScaleZ = EditorGUILayout.FloatField("Z", _scale.z, GUILayout.Width(60));
        EditorGUI.showMixedValue = false;

        EditorGUIUtility.labelWidth = oldLabelWidth;
        EditorGUILayout.EndHorizontal();
        Vector3 newScale = new Vector3(newScaleX, newScaleY, newScaleZ);

        // Apply changes (relative modification)
        foreach (var obj in _selectedObjects)
        {
            Undo.RecordObject(obj.transform, "Modify Transform");
            if (newPosition != _position)
                obj.transform.position += newPosition - _position;
            if (newRotation != _rotation)
                obj.transform.eulerAngles += newRotation - _rotation;
            if (newScale != _scale)
                obj.transform.localScale += newScale - _scale;
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawComponentEditor()
    {
        EditorGUILayout.LabelField("Add/Remove Component", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Add Component", GUILayout.Width(120)))
        {
            GenericMenu menu = new GenericMenu();
            foreach (Type type in GetAllComponentTypes())
            {
                menu.AddItem(new GUIContent(type.Name), false, () => AddComponentToSelected(type));
            }
            menu.ShowAsContext();
        }
        if (GUILayout.Button("Remove Component", GUILayout.Width(120)))
        {
            GenericMenu menu = new GenericMenu();
            // List the component types (excluding Transform) present in the selected objects
            HashSet<Type> compTypes = new HashSet<Type>();
            foreach (var obj in _selectedObjects)
            {
                foreach (Component comp in obj.GetComponents<Component>())
                {
                    if (!(comp is Transform))
                        compTypes.Add(comp.GetType());
                }
            }
            List<Type> compList = compTypes.ToList();
            compList.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.Ordinal));
            foreach (Type type in compList)
            {
                menu.AddItem(new GUIContent(type.Name), false, () => RemoveComponentFromSelected(type));
            }
            menu.ShowAsContext();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DetermineCommonTransformValues()
    {
        if (_selectedObjects.Count == 0)
            return;

        _position = _selectedObjects[0].transform.position;
        _rotation = _selectedObjects[0].transform.eulerAngles;
        _scale = _selectedObjects[0].transform.localScale;

        for (int i = 0; i < 3; i++)
        {
            _positionMixed[i] = _selectedObjects.Any(o => !Mathf.Approximately(o.transform.position[i], _position[i]));
            _rotationMixed[i] = _selectedObjects.Any(o => !Mathf.Approximately(o.transform.eulerAngles[i], _rotation[i]));
            _scaleMixed[i] = _selectedObjects.Any(o => !Mathf.Approximately(o.transform.localScale[i], _scale[i]));
        }
    }

    // Collects component types from all loaded assemblies (non-abstract, public)
    private static List<Type> _allComponentTypes;
    private static List<Type> GetAllComponentTypes()
    {
        if (_allComponentTypes == null)
        {
            _allComponentTypes = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch
                {
                    continue;
                }
                foreach (Type type in types)
                {
                    if (typeof(Component).IsAssignableFrom(type) && !type.IsAbstract && type.IsPublic)
                    {
                        _allComponentTypes.Add(type);
                    }
                }
            }
            _allComponentTypes.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.Ordinal));
        }
        return _allComponentTypes;
    }

    private void AddComponentToSelected(Type type)
    {
        foreach (var obj in _selectedObjects)
        {
            if (obj.GetComponent(type) == null)
            {
                Undo.AddComponent(obj, type);
            }
        }
    }

    private void RemoveComponentFromSelected(Type type)
    {
        foreach (var obj in _selectedObjects)
        {
            Component comp = obj.GetComponent(type);
            if (comp != null)
            {
                Undo.DestroyObjectImmediate(comp);
            }
        }
    }

    private void RefreshGameObjects()
    {
        _gameObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                        .Where(obj => obj.hideFlags == HideFlags.None)
                        .ToList();
        _lastObjectCount = _gameObjects.Count;
    }

    private void AutoRefresh()
    {
        int currentCount = Resources.FindObjectsOfTypeAll<GameObject>()
                            .Count(obj => obj.hideFlags == HideFlags.None);
        if (currentCount != _lastObjectCount)
        {
            RefreshGameObjects();
            Repaint();
        }
    }
}
