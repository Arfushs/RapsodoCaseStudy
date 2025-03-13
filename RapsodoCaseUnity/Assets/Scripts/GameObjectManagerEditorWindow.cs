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
        // GameObject listesini çizdirme
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
        
        // Seçili objelerin transform değerlerini düzenleme
        if (_selectedObjects.Count > 0)
        {
            EditorGUILayout.LabelField("Edit Selected GameObjects", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            DetermineCommonTransformValues();

            // Pozisyon 
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

            // Rotasyon 
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
            
            // Transform değerlerini uygulama
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
