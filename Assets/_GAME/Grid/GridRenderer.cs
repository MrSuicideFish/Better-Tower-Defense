#pragma warning disable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(GameGridManager))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteAlways]
public class GridRenderer : MonoBehaviour
{
    private int _GridSizeX = 500;
    private int _GridSizeY = 500;

    private GameGridManager _grid;
    private MeshRenderer _renderer;
    private Material _material;
    private MeshFilter _meshFilter;
    private Mesh _gridPlane;

    [Range(0, 30)]
    public int gridResolution = 1;

    private void OnEnable()
    {
        _grid = this.GetComponent<GameGridManager>();
        _renderer = this.GetComponent<MeshRenderer>();
        _meshFilter = this.GetComponent<MeshFilter>();
        _material = _renderer.sharedMaterial;

        _GridSizeX = _grid.gridSizeX;
        _GridSizeY = _grid.gridSizeY;
        
        Rebuild();
        _renderer.enabled = true;
        _grid.OnGameGridUpdated += UpdateShaderBuffers;
    }
    
    private void OnDisable()
    {
        _renderer.enabled = false;
    }

    private void OnValidate()
    {
        Rebuild();
    }

    private void Update()
    {
        Vector4 mouseCoordPosition = new Vector4();
        if (GameManager.Instance.currentGameState == GameState.PreWave)
        {
            var mouseScreenPos = Input.mousePosition;
            Ray mouseRay = Camera.main.ScreenPointToRay(mouseScreenPos);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float enter = 0;
            if (groundPlane.Raycast(mouseRay, out enter))
            {
                Vector3 point = mouseRay.GetPoint(enter);
                int coordX = (int) (Mathf.RoundToInt(point.x / _grid.gridSpaceSize) * _grid.gridSpaceSize);
                int coordy = (int) (Mathf.RoundToInt(point.z / _grid.gridSpaceSize) * _grid.gridSpaceSize);
                
                mouseCoordPosition = new Vector4(coordX, 0, coordy, 0);
            }
        }

        _material.SetFloat("_gridSize", _grid.gridSpaceSize);
        _material.SetVector("_mouseCoordPosition", mouseCoordPosition);
    }

    public void Rebuild()
    {
        if (!_meshFilter) return;
        _gridPlane = new Mesh();
        List<Vector3> verts = new List<Vector3>()
        {
            new Vector3(-_GridSizeX, 0, -_GridSizeY),
            new Vector3(_GridSizeX, 0, -_GridSizeY),
            new Vector3(-_GridSizeX, 0, _GridSizeY),
            new Vector3(_GridSizeX, 0, _GridSizeY)
        };
        
        Vector3[] normals = new Vector3[4]
        {
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up
        };
        
        int[] tris = new int[]
        {
            2,1,0,
            3,1,2
        };
        
        Vector2[] uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };

        _gridPlane.vertices = verts.ToArray();
        _gridPlane.triangles = tris;
        _gridPlane.normals = normals;
        _gridPlane.uv = uv;

        int res = gridResolution;
        if (res < 0) res = 0;
        MeshHelper.Subdivide(_gridPlane, gridResolution * 4);
        _meshFilter.mesh = _gridPlane;
    }

    public void UpdateShaderBuffers()
    {
        if (_grid == null || _material == null)
        {
            return;
        }

        List<Vector4> vacantPositions = new List<Vector4>();
        foreach (Vector3 pos in _grid.GetVacantGridSpacePositions())
            vacantPositions.Add(new Vector4(pos.x, pos.y, pos.z, 0));

        List<Vector4> occupiedPositions = new List<Vector4>();
        foreach (Vector3 pos in _grid.GetOccupiedGridSpacePositions())
            occupiedPositions.Add(new Vector4(pos.x, pos.y, pos.z, 0));

        List<Vector4> unbuildablePositions = new List<Vector4>();
        foreach (Vector3 pos in _grid.GetUnbuildableGridSpacePositions())
            unbuildablePositions.Add(new Vector4(pos.x, pos.y, pos.z, 0));

        if (vacantPositions.Count > 0)
            _material.SetVectorArray("_vacantPositions", vacantPositions);

        if (occupiedPositions.Count > 0)
            _material.SetVectorArray("_occupiedPositions", occupiedPositions);
        
        if (unbuildablePositions.Count > 0)
            _material.SetVectorArray("_unbuildablePositions", unbuildablePositions);
    }

    private void OnRenderObject()
    {
        _material.SetPass(0);
    }
}
#pragma warning enable