using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum GridSpaceType
{
    Empty = 0,
    Buildable = 1,
    Unbuildable = 2
}

[Serializable]
public enum GridSpaceState
{
    None = -1,
    Vacant = 0,
    Occupied = 1
}

[Serializable]
public class GameGridSpace
{
    [SerializeField] public GridSpaceType spaceType;
    [SerializeField] public GridSpaceState spaceState;
}

[Serializable]
public class GameGridLayer
{
    // slot
    [SerializeField] public GameGridSpace[] layerSpaces;
    [SerializeField] public int gridSizeX, gridSizeY;

    public GameGridLayer(int sizeX, int sizeY)
    {
        gridSizeX = sizeX;
        gridSizeY = sizeY;

        layerSpaces = new GameGridSpace[gridSizeX * gridSizeY];
        for (int i = 0; i < layerSpaces.Length; i++)
        {
            GameGridSpace gridSpace = new GameGridSpace();
            gridSpace.spaceState = GridSpaceState.None;
            gridSpace.spaceType = GridSpaceType.Empty;

            layerSpaces[i] = gridSpace;
        }
    }
}

[Serializable]
public class GameGrid
{
    [SerializeField] private int _sizeX, _sizeY, _layerCount;
    [SerializeField] public GameGridLayer[] layers;

    public GameGrid(int sizeX, int sizeY, int numOfLayers)
    {
        _sizeX = sizeX;
        _sizeY = sizeY;
        _layerCount = numOfLayers;
        
        layers = new GameGridLayer[_layerCount];
        for (var i = 0; i < _layerCount; i++)
        {
            layers[i] = new GameGridLayer(sizeX, sizeY);
        }
    }

    public int GetSizeX(){return _sizeX;}
    public int GetSizeY(){return _sizeY;}
    public int GetLayerCount(){return _layerCount;}

    public static Color SpaceTypeToColor(GridSpaceType spaceType)
    {
        switch (spaceType)
        {
            case GridSpaceType.Empty:
                return Color.white;
            case GridSpaceType.Buildable:
                return Color.green;
            case GridSpaceType.Unbuildable:
                return Color.red;
        }

        return Color.white;
    }
}