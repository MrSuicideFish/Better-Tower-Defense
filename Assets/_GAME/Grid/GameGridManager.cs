using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GameGridManager : MonoBehaviour
{
    private static GameGridManager _instance;

    public static GameGridManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gridObj = GameObject.FindWithTag("GameGrid");
                if (gridObj == null)
                {
                    Debug.LogError("Cant find grid!");
                    return null;
                }
                
                _instance = gridObj.GetComponent<GameGridManager>();
            }
            return _instance;
        }
    }
    
    public int gridSizeX, gridSizeY, gridLayerCount;

    public float gridSpaceSize = 5.0f;
    public float gridLayerHeight = 1.0f;
    [SerializeField] public GameGrid _currentGrid;

    public delegate void GameGridDelegate();
    public event GameGridDelegate OnGameGridUpdated;
    
    public void CreateNewGrid(int sizeX, int sizeY, int layers)
    {
        _currentGrid = new GameGrid(sizeX, sizeY, layers);
    }

    public void ResizeGrid(int newSizeX, int newSizeY, int layers)
    {
        CreateNewGrid(newSizeX, newSizeY, layers);
    }

    public bool IsSpaceBuildable(int x, int y, int layer)
    {
        if (layer < 0 || layer >= gridLayerCount) return false;
        GameGridLayer gridLayer = _currentGrid.layers[layer];
        if (gridLayer != null)
        {
            int idx = x + gridSizeX * y;
            if (idx < 0 || idx >= gridLayer.layerSpaces.Length) return false;
            
            var layerSpace = gridLayer.layerSpaces[idx];
            if (layerSpace != null)
            {
                return layerSpace.spaceType == GridSpaceType.Buildable 
                       && layerSpace.spaceState == GridSpaceState.Vacant;   
            }
        }
        return false;
    }

    public GridSpaceType GetGridSpaceType(int x, int y, int layer)
    {
        if (_currentGrid != null && layer < 0 || layer >= gridLayerCount) return GridSpaceType.Empty;
        
        GameGridLayer gridLayer = _currentGrid.layers[layer];
        if (gridLayer != null)
        {
            int idx = _currentGrid.GetSizeX() * y + x;
            if (idx < 0 || idx >= gridLayer.layerSpaces.Length) return GridSpaceType.Empty;
            if (gridLayer.layerSpaces[idx] != null)
            {
                return gridLayer.layerSpaces[idx].spaceType;
            }
        }

        return GridSpaceType.Empty;
    }
    
    public void SetGridSpaceType(GridSpaceType newType, int x, int y, int layer)
    {
        if (layer < 0 || layer >= gridLayerCount) return;
        
        GameGridLayer gridLayer = _currentGrid.layers[layer];
        if (gridLayer != null)
        {
            int idx = _currentGrid.GetSizeX() * y + x;
            if (idx < 0 || idx >= gridLayer.layerSpaces.Length) return;
            if (gridLayer.layerSpaces[idx] != null)
            {
                gridLayer.layerSpaces[idx].spaceType = newType;
            }
        }
    }

    public GridSpaceState GetGridSpaceState(int x, int y, int layer)
    {
        if (layer < 0 || layer >= gridLayerCount) return GridSpaceState.None;
        
        GameGridLayer gridLayer = _currentGrid.layers[layer];
        if (gridLayer != null)
        {
            int idx = _currentGrid.GetSizeX() * y + x;
            if (idx < 0 || idx >= gridLayer.layerSpaces.Length) return GridSpaceState.None;
            if (gridLayer.layerSpaces[idx] != null)
            {
                return gridLayer.layerSpaces[idx].spaceState;
            }
        }

        return GridSpaceState.None;
    }
    
    public void SetGridSpaceState(GridSpaceState newState, int x, int y, int layer)
    {
        if (layer < 0 || layer >= gridLayerCount) return;
        
        GameGridLayer gridLayer = _currentGrid.layers[layer];
        if (gridLayer != null)
        {
            int idx = _currentGrid.GetSizeX() * y + x;
            if (idx < 0 || idx >= gridLayer.layerSpaces.Length) return;
            if (gridLayer.layerSpaces[idx] != null)
            {
                gridLayer.layerSpaces[idx].spaceState = newState;
            }
        }
    }

    public void ShowGameplayGrid()
    {
        this.gameObject.SetActive(true);
    }
    
    public void HideGameplayGrid()
    {
        this.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (_gridUpdateRoutine != null)
        {
            StopCoroutine(_gridUpdateRoutine);
            _gridUpdateRoutine = null;
        }
        
        if (Application.isPlaying)
        {
            _gridUpdateRoutine = StartCoroutine(KeepSpaceBuffersUpToDate());
        }
    }
    
    
    #region Runtime Grid Buffers 
    [Range(0.1f, 3)]
    public float gridUpdateRate = 1;
    
    // Keep track of grid state info once so we don't have
    // to keep iterating through the grid for other things
    private Coroutine _gridUpdateRoutine;
    
    // Space Buffers
    private List<Vector2> _occupiedGridSpaces = new List<Vector2>();
    private List<Vector3> _occupiedGridSpacePositions = new List<Vector3>();
    private List<Vector2> _vacantGridSpaces = new List<Vector2>();
    private List<Vector3> _vacantGridSpacePositions = new List<Vector3>();
    private List<Vector2> _unbuildableGridSpaces = new List<Vector2>();
    private List<Vector3> _unbuildableGridSpacePositions = new List<Vector3>();
    public List<Vector2> GetOccupiedGridSpaces() {return _occupiedGridSpaces;}
    public List<Vector3> GetOccupiedGridSpacePositions() {return _occupiedGridSpacePositions;}
    public List<Vector2> GetVacantGridSpaces() {return _vacantGridSpaces;}
    public List<Vector3> GetVacantGridSpacePositions() {return _vacantGridSpacePositions;}
    public List<Vector2> GetUnbuildableGridSpaces() {return _unbuildableGridSpaces;}
    public List<Vector3> GetUnbuildableGridSpacePositions() {return _unbuildableGridSpacePositions;}

    private IEnumerator KeepSpaceBuffersUpToDate()
    {
        float t = 0;
        while (true)
        {
            if (t >= gridUpdateRate)
            {
                UpdateSpacesBuffers();
                OnGameGridUpdated?.Invoke();
                t = 0.0f;
            }
            t += Time.deltaTime;
            yield return null;
        }
        yield return null;
    }
    
    private void UpdateSpacesBuffers()
    {
        if (_currentGrid == null) return;
        
        _occupiedGridSpaces.Clear();
        _occupiedGridSpacePositions.Clear(); 
        
        _vacantGridSpaces.Clear();
        _vacantGridSpacePositions.Clear();
        
        _unbuildableGridSpaces.Clear();
        _unbuildableGridSpacePositions.Clear();

        for (int i = 0; i < _currentGrid.layers[0].layerSpaces.Length; i++)
        {
            GameGridSpace space = _currentGrid.layers[0].layerSpaces[i];
            GridSpaceType spaceType = space.spaceType;
            GridSpaceState spaceState = space.spaceState;
            
            int x = i % _currentGrid.GetSizeX();
            int y = i / _currentGrid.GetSizeY();

            Vector2 gridPos = new Vector2(x, y);
            Vector3 worldPos = new Vector3(x, transform.position.y * 0, y);
            worldPos.x *= gridSpaceSize;
            worldPos.z *= gridSpaceSize;
            //worldPos.y *= gridLayerHeight;

            switch (spaceType)
            {
                case GridSpaceType.Empty:
                    break;
                case GridSpaceType.Buildable:
                    if (spaceState == GridSpaceState.Occupied)
                    {
                        if (!_occupiedGridSpaces.Contains(gridPos))
                        {
                            _occupiedGridSpaces.Add(gridPos);
                            _occupiedGridSpacePositions.Add(worldPos);
                        }
                    }
                    else if (spaceState == GridSpaceState.Vacant)
                    {
                        if (!_vacantGridSpaces.Contains(gridPos))
                        {
                            _vacantGridSpaces.Add(gridPos);
                            _vacantGridSpacePositions.Add(worldPos);
                        }
                    }
                    break;
                case GridSpaceType.Unbuildable:
                    if (!_unbuildableGridSpaces.Contains(gridPos))
                    {
                        _unbuildableGridSpaces.Add(gridPos);
                        _unbuildableGridSpacePositions.Add(worldPos);
                    }
                    break;
            }
        }
    }
    #endregion
    
    /*

     */
}
