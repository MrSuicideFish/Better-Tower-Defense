using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

public class GameGridEditor : EditorWindow
{
    private abstract class EditorSection
    {
        public string sectionTitle;
        public bool isExpanded = true;
        public bool canBeDisabled;
        public bool isDisabled;
    }

    private class CreateGridEditorSection : EditorSection
    {
        public int gridSizeX;
        public int gridSizeY;
        public int gridLayerCount;

        public CreateGridEditorSection(string title)
        {
            sectionTitle = title;
        }
    }

    private class PaintGridEditorSection : EditorSection
    {
        public GridSpaceType typeToPaint;
        public int layerToPaint;
        public PaintGridEditorSection(string title)
        {
            sectionTitle = title;
        }
    }

    private class VisualizationEditorSection : EditorSection
    {
        public Color gridOutlineColor = new Color(0.3065238f, 0, 1.0f, 1.0f);
        public bool showGrid = true;
        public float gridSpaceOpacity = 0.02f;
        
        public VisualizationEditorSection(string title)
        {
            sectionTitle = title;
        }
    }

    private static GameObject _managerObj;
    private static GameGridManager _gridManager;
    private static SerializedObject _serializedGridManager;

    private static SerializedProperty _gridSizeX;
    private static SerializedProperty _gridSizeY;
    private static SerializedProperty _gridLayerCount;
    private static SerializedProperty _gridSpaceSize;
    private static SerializedProperty _gridLayerHeight;

    private static GameGridEditor _gridEditor;
    private bool _gridEditMode = false;

    #region UI
    private static CreateGridEditorSection createGridOptions;
    private static PaintGridEditorSection paintGridOptions;
    private static VisualizationEditorSection visualizationOptions;

    private GUIContent paintEmptyIcon;
    private GUIContent paintBuildableIcon;
    private GUIContent paintUnbuildableIcon;

    private GUIStyle paintTypeBtnStyle;
    #endregion

    [MenuItem("Tower Defense/Grid Editor")]
    public static void ShowWindow()
    {
        _gridEditor = EditorWindow.GetWindow<GameGridEditor>();
        _gridEditor.position = new Rect(200, 200, 300, 500);
        _gridEditor.ShowAuxWindow();
    }
    
    public void OnEnable()
    {
        SceneView.duringSceneGui -= OnSceneView;
        SceneView.duringSceneGui += OnSceneView;
        
        LoadResources();
        TryFindGameGrid();
        InitProperties();
        ToggleGridView(true);
        ToggleGridEditMode(false);
    }

    public void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneView;
        ToggleGridView(false);
    }

    public void OnGUI()
    {
        if (_serializedGridManager == null || _managerObj == null)
        {
            GUILayout.Label("Grid Manager Not Found! Make sure to tag it 'GameGrid'!");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh"))
            {
                TryFindGameGrid();
                InitProperties();
            }
            return;
        }
        
        if (DrawSection(createGridOptions))
        {
            createGridOptions.gridSizeX = EditorGUILayout.IntSlider("Grid Size X", createGridOptions.gridSizeX, 5, 50);
            createGridOptions.gridSizeY = EditorGUILayout.IntSlider("Grid Size Y", createGridOptions.gridSizeY, 5, 50);
            createGridOptions.gridLayerCount = EditorGUILayout.IntSlider("Layers", createGridOptions.gridLayerCount, 1, 3);
            
            if (GUILayout.Button("Create New"))
            {
                _gridManager.CreateNewGrid(
                    createGridOptions.gridSizeX,
                    createGridOptions.gridSizeY,
                    createGridOptions.gridLayerCount
                );
            }
            
            if (GUILayout.Button("Resize"))
            {
                _gridManager.ResizeGrid(
                    createGridOptions.gridSizeX,
                    createGridOptions.gridSizeY,
                    createGridOptions.gridLayerCount);
            }
        }
        
        //GUILayout.FlexibleSpace();

        if (DrawSection(paintGridOptions))
        {
            paintTypeBtnStyle = new GUIStyle(EditorStyles.miniButton);
            paintTypeBtnStyle.fixedWidth = 64;
            paintTypeBtnStyle.fixedHeight = 64;
            
            GUILayout.Label("Paint Cell Type:", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();

            Color oldGuiColor = GUI.color;
            GUILayout.FlexibleSpace();
            if (paintGridOptions.typeToPaint == GridSpaceType.Empty) GUI.color = Color.green;
            if (GUILayout.Button(paintEmptyIcon, paintTypeBtnStyle))
            {
                paintGridOptions.typeToPaint = GridSpaceType.Empty;
            }
            
            GUI.color = oldGuiColor;
            GUILayout.FlexibleSpace();
            if (paintGridOptions.typeToPaint == GridSpaceType.Buildable) GUI.color = Color.green;
            if (GUILayout.Button(paintBuildableIcon, paintTypeBtnStyle))
            {
                paintGridOptions.typeToPaint = GridSpaceType.Buildable;
            }
            
            GUI.color = oldGuiColor;
            GUILayout.FlexibleSpace();
            if (paintGridOptions.typeToPaint == GridSpaceType.Unbuildable) GUI.color = Color.green;
            if (GUILayout.Button(paintUnbuildableIcon, paintTypeBtnStyle))
            {
                paintGridOptions.typeToPaint = GridSpaceType.Unbuildable;
            }

            GUI.color = oldGuiColor;
            
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
            paintGridOptions.layerToPaint = EditorGUILayout.IntField("Paint Layer", paintGridOptions.layerToPaint);
            
            if (_gridEditMode == true)
            {
                GUI.color = Color.green;
            }
            if (GUILayout.Button("Paint Grid"))
            {
                _gridEditMode = !_gridEditMode;
            }
            GUI.color = oldGuiColor;
            //paintGridOptions.typeToPaint = (GridSpaceType)EditorGUILayout.EnumPopup("Cell Type:", paintGridOptions.typeToPaint);
        }

        if (DrawSection(visualizationOptions))
        {
            visualizationOptions.showGrid
                = GUILayout.Toggle(visualizationOptions.showGrid, "Show Grid");
            
            visualizationOptions.gridOutlineColor
                = EditorGUILayout.ColorField("Grid Outline Color", visualizationOptions.gridOutlineColor);

            visualizationOptions.gridSpaceOpacity
                = EditorGUILayout.Slider("Grid Space Opacity", visualizationOptions.gridSpaceOpacity, 0.0f, 1.0f);
        }
    }

    private void LoadResources()
    {
        if (createGridOptions == null)
        {
            createGridOptions = new CreateGridEditorSection("Create / Resize");
            createGridOptions.canBeDisabled = false;
        }

        if (paintGridOptions == null)
        {
            paintGridOptions = new PaintGridEditorSection("Paint / Edit");
            paintGridOptions.canBeDisabled = false;
        }

        if (visualizationOptions == null)
        {
            visualizationOptions = new VisualizationEditorSection("Visuals");
            visualizationOptions.canBeDisabled = false;
        }
        
        paintEmptyIcon = EditorGUIUtility.IconContent("Collab.Build");
        paintBuildableIcon = EditorGUIUtility.IconContent("Collab.BuildSucceeded");
        paintUnbuildableIcon = EditorGUIUtility.IconContent("Collab.BuildFailed");
    }

    private void InitProperties()
    {
        if (_serializedGridManager == null) return;
        _gridSizeX = _serializedGridManager.FindProperty("gridSizeX");
        _gridSizeY = _serializedGridManager.FindProperty("gridSizeY");
        _gridLayerCount = _serializedGridManager.FindProperty("gridLayerCount");
        _gridSpaceSize = _serializedGridManager.FindProperty("gridSpaceSize");
        _gridLayerHeight = _serializedGridManager.FindProperty("gridLayerHeight");
    }
    
    private void TryFindGameGrid()
    {
        if (_managerObj == null)
        {
            _managerObj = GameObject.FindWithTag("GameGrid");
        }
        
        _gridManager = _managerObj.GetComponent<GameGridManager>();
        if (_gridManager != null)
        {
            _serializedGridManager = new SerializedObject(_gridManager);
        }
    }

    private void ToggleGridView(bool isOn)
    {
        SceneView sceneView = SceneView.currentDrawingSceneView;
        if (sceneView != null)
        {
            sceneView.drawGizmos = !isOn;
            sceneView.showGrid = !isOn;   
        }
    }
    
    private void ToggleGridEditMode(bool isOn)
    {
        _gridEditMode = isOn;
    }

    private GUIStyle sectionHeaderStyle;
    private GUIStyle sectionHeaderCloseStyle;
    private bool DrawSection(EditorSection section)
    {
        sectionHeaderStyle = new GUIStyle(GUIStyle.none);
        sectionHeaderStyle.normal.textColor = Color.white;
        
        sectionHeaderCloseStyle = new GUIStyle(GUI.skin.label);
        
        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.BeginHorizontal();
            //GUILayout.Label(section.sectionTitle, EditorStyles.largeLabel);
            if (GUILayout.Button(new GUIContent(section.sectionTitle), sectionHeaderStyle, GUILayout.MinWidth(position.width - 50)))
            {
                section.isExpanded = !section.isExpanded;
            }

            //GUILayout.FlexibleSpace();
            if (GUILayout.Button(section.isExpanded ? "-" : "+", GUIStyle.none))
            {
                section.isExpanded = !section.isExpanded;
            }

            if (section.canBeDisabled)
            {
                section.isDisabled = !GUILayout.Toggle(section.isDisabled, "");
            }
            EditorGUILayout.EndHorizontal();
        }
        if (section.canBeDisabled && section.isDisabled) return false;
        return section.isExpanded;
    }

    private void OnSceneView(SceneView sceneView)
    {
        if (_gridManager != null && _serializedGridManager != null)
        {
            if (Camera.current != null)
            {
                Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                Plane layerPlane = new Plane(Vector3.up, Vector3.up * (_gridManager.gridLayerHeight * paintGridOptions.layerToPaint));
                float layer0Enter = 0;
                if (layerPlane.Raycast(mouseRay, out layer0Enter))
                {
                    float gridSpaceSize = _gridSpaceSize.floatValue;
                    Vector3 point = mouseRay.GetPoint(layer0Enter);
                    paintPreviewCoordX = (int) (Mathf.RoundToInt(point.x / gridSpaceSize) * gridSpaceSize);
                    paintPreviewCoordY = (int) (Mathf.RoundToInt(point.z / gridSpaceSize) * gridSpaceSize);
                }
            }   
        }

        DrawWorldGrid();
    }
    
    // painting preview
    private int paintPreviewCoordX, paintPreviewCoordY;

    // previous draw params
    private int lastSizeX, lastSizeY, lastLayerCount;
    private float lastGridLayerHeight;
    
    // draw points
    private Vector3 drawGridSpacePos;
    private Vector3 topLeft;
    private Vector3 topRight;
    private Vector3 bottomLeft;
    private Vector3 bottomRight;
    private Vector3[] points;
    private void DrawWorldGrid()
    {
        if (!visualizationOptions.showGrid 
            || _gridManager == null
            || _gridManager._currentGrid == null) return;

        /*
        bool isDirty = lastSizeX != _gridManager._currentGrid.GetSizeX()
                       || lastSizeY != _gridManager._currentGrid.GetSizeY()
                       || lastLayerCount != _gridManager._currentGrid.GetLayerCount()
                       || lastGridLayerHeight != _gridManager.gridLayerHeight;
        */

        for (int l = 0; l < 1; l++)
        {
            for (int i = 0; i < _gridManager._currentGrid.layers[l].layerSpaces.Length; i++)
            {
                int x = i % _gridManager._currentGrid.GetSizeX();
                int y = i / _gridManager._currentGrid.GetSizeY();
                
                drawGridSpacePos = new Vector3(x, _gridManager.transform.position.y * l, y);
                drawGridSpacePos.x *= _gridSpaceSize.floatValue;
                drawGridSpacePos.z *= _gridSpaceSize.floatValue;
                drawGridSpacePos.y *= _gridManager.gridLayerHeight;
                
                topLeft = drawGridSpacePos;
                topRight = drawGridSpacePos;
                bottomLeft = drawGridSpacePos;
                bottomRight = drawGridSpacePos;

                float cubeHalf = _gridSpaceSize.floatValue / 2f;

                topLeft.x -= cubeHalf;
                topLeft.z += cubeHalf;
                
                topRight.x += cubeHalf;
                topRight.z += cubeHalf;
                
                bottomLeft.x -= cubeHalf;
                bottomLeft.z -= cubeHalf;
                
                bottomRight.x += cubeHalf;
                bottomRight.z -= cubeHalf;

                points = new Vector3[]
                {
                    topLeft,
                    topRight,
                    bottomRight,
                    bottomLeft
                };

                GridSpaceType gridSpaceType = _gridManager.GetGridSpaceType(x, y, l);
                Color spaceColor = GameGrid.SpaceTypeToColor(gridSpaceType);
                
                if (gridSpaceType == GridSpaceType.Buildable || gridSpaceType == GridSpaceType.Unbuildable)
                {
                    spaceColor.a = visualizationOptions.gridSpaceOpacity * 10.0f;
                }
                else
                {
                    spaceColor.a =  visualizationOptions.gridSpaceOpacity;
                }

                if (_gridEditMode && l == paintGridOptions.layerToPaint
                     && (int)drawGridSpacePos.x == paintPreviewCoordX
                     && (int)drawGridSpacePos.z == paintPreviewCoordY)
                {
                    spaceColor = GameGrid.SpaceTypeToColor(paintGridOptions.typeToPaint);

                    Event e = Event.current;
                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        _gridManager.SetGridSpaceType(paintGridOptions.typeToPaint, x, y, paintGridOptions.layerToPaint);
                        GridSpaceState stateToSet = paintGridOptions.typeToPaint == GridSpaceType.Buildable ? 
                            GridSpaceState.Vacant : GridSpaceState.None;
                        _gridManager.SetGridSpaceState(stateToSet, x, y, paintGridOptions.layerToPaint);
                        continue;
                    }
                }
                
                Handles.color = spaceColor;
                Handles.DrawAAConvexPolygon(points);
                
                Handles.color = visualizationOptions.gridOutlineColor;
                Handles.DrawAAPolyLine(points);
            }
        }
        
        lastSizeX = _gridManager._currentGrid.GetSizeX();
        lastSizeY = _gridManager._currentGrid.GetSizeY();
        lastLayerCount = _gridManager._currentGrid.GetLayerCount();
        lastGridLayerHeight = _gridManager.gridLayerHeight;
    }
}