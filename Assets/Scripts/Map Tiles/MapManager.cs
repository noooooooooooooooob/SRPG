using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public CharacterSpawner characterSpawner;
    public int width;
    public int height;
    public Transform mapRoot; // 맵 오브젝트 참조
    public GridTile[,] grid;

    void Start()
    {
        BuildGridFromHierarchy();
    }
    public IEnumerator GenerateMapCoroutine()
    {
        yield return StartCoroutine(characterSpawner.SpawnCharactersWithEffect());
    }
    void BuildGridFromHierarchy()
    {
        grid = new GridTile[width, height];

        GridTile[] gridTiles = mapRoot.GetComponentsInChildren<GridTile>();

        foreach (GridTile tile in gridTiles)
        {
            Vector2Int pos = tile.gridPos; // GridTile에 gridPosition이라는 좌표 필드가 있다고 가정

            if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
            {
                grid[pos.x, pos.y] = tile;
                tile.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"Tile at {pos} is out of grid bounds.");
            }
        }
    }
    public GridTile GetTile(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x >= width || pos.y >= height)
            return null;
        return grid[pos.x, pos.y];
    }
}