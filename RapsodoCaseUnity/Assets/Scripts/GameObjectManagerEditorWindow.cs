using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class GameObjectManager : EditorWindow
{
    private List<GameObject> _gameObjects = new List<GameObject>();
    private Vector2 _scrollPos;
    private int _lastObjectCount;
    private List<GameObject> _selectedObjects = new List<GameObject>();
    private Vector3 _position;
    private Vector3 _rotation;
    private Vector3 _scale;
    private bool[] _positionMixed = new bool[3];
    private bool[] _rotationMixed = new bool[3];
    private bool[] _scaleMixed = new bool[3];

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
        EditorGUILayout.LabelField("GameObject List", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");
        
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        foreach (var obj in _gameObjects)
        {
            if (obj != null)
            {
                EditorGUILayout.BeginHorizontal("box");
                bool newState = EditorGUILayout.Toggle(obj.activeSelf, GUILayout.Width(20));
                if (newState != obj.activeSelf)
                {
                    Undo.RecordObject(obj, "Toggle Active State");
                    obj.SetActive(newState);
                }
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
        EditorGUILayout.Space();
        
        if (_selectedObjects.Count > 0)
        {
            EditorGUILayout.LabelField("Edit Selected GameObjects", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            DetermineCommonTransformValues();

            // Pozisyon
            EditorGUI.showMixedValue = _positionMixed[0] || _positionMixed[1] || _positionMixed[2];
            Vector3 newPosition = EditorGUILayout.Vector3Field("Position", _position);
            EditorGUI.showMixedValue = false;

            // Rotasyon
            EditorGUI.showMixedValue = _rotationMixed[0] || _rotationMixed[1] || _rotationMixed[2];
            Vector3 newRotation = EditorGUILayout.Vector3Field("Rotation", _rotation);
            EditorGUI.showMixedValue = false;

            // Ölçek
            EditorGUI.showMixedValue = _scaleMixed[0] || _scaleMixed[1] || _scaleMixed[2];
            Vector3 newScale = EditorGUILayout.Vector3Field("Scale", _scale);
            EditorGUI.showMixedValue = false;
            
            foreach (var obj in _selectedObjects)
            {
                Undo.RecordObject(obj.transform, "Modify Transform");
                if (newPosition != _position) obj.transform.position += newPosition - _position;
                if (newRotation != _rotation) obj.transform.eulerAngles += newRotation - _rotation;
                if (newScale != _scale) obj.transform.localScale += newScale - _scale;
            }
            
            EditorGUILayout.EndVertical();
        }
    }

    private void DetermineCommonTransformValues()
    {
        if (_selectedObjects.Count == 0) return;
        
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

    private void RefreshGameObjects()
    {
        _gameObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.hideFlags == HideFlags.None).ToList();
        _lastObjectCount = _gameObjects.Count;
    }

    private void AutoRefresh()
    {
        int currentCount = Resources.FindObjectsOfTypeAll<GameObject>().Count(obj => obj.hideFlags == HideFlags.None);
        if (currentCount != _lastObjectCount)
        {
            RefreshGameObjects();
            Repaint();
        }
    }
}