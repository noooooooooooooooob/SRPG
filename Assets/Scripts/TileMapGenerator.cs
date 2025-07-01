using DG.Tweening;
using UnityEngine;

public class TileMapGenerator : MonoBehaviour
{
    public GameObject tilePrefab;
    public Transform startPos;
    public int width;
    public int height;
    public float spacing;

    public void GenerateMap()
    {
        ClearMap();  // 기존 타일 제거

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 pos = new Vector3(startPos.position.x + x * spacing, startPos.position.y + y * spacing, 0);
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                tile.name = $"Tile_{x}_{y}";
                tile.GetComponent<GridTile>().gridPos = new Vector2Int(x, y);
            }
        }
    }

    public void ClearMap()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
    public void SetTilesSetting()
    {
        GridTile[,] tiles = new GridTile[width, height];

        // 모든 타일을 배열로 수집
        foreach (Transform child in transform)
        {
            GridTile tile = child.GetComponent<GridTile>();
            if (tile != null)
            {
                Vector2Int pos = tile.gridPos;
                tiles[pos.x, pos.y] = tile;
            }
        }

        // 각 타일에 대해 방향 설정
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GridTile tile = tiles[x, y];
                if (tile == null) continue;

                // Negative 타일이면 전 방향 이동 불가
                if (tile.tileType == TileType.Negative)
                {
                    tile.canGoUp = false;
                    tile.canGoDown = false;
                    tile.canGoLeft = false;
                    tile.canGoRight = false;
                    continue;
                }

                int tileLayer = tile.GetTileLayer();

                // 위
                if (y + 1 < height && tiles[x, y + 1] != null &&
                    tiles[x, y + 1].tileType != TileType.Negative &&
                    tiles[x, y + 1].GetTileLayer() == tileLayer)
                {
                    tile.canGoUp = true;
                }
                else tile.canGoUp = false;

                // 아래
                if (y - 1 >= 0 && tiles[x, y - 1] != null &&
                    tiles[x, y - 1].tileType != TileType.Negative &&
                    tiles[x, y - 1].GetTileLayer() == tileLayer)
                {
                    tile.canGoDown = true;
                }
                else tile.canGoDown = false;

                // 왼쪽
                if (x - 1 >= 0 && tiles[x - 1, y] != null &&
                    tiles[x - 1, y].tileType != TileType.Negative &&
                    tiles[x - 1, y].GetTileLayer() == tileLayer)
                {
                    tile.canGoLeft = true;
                }
                else tile.canGoLeft = false;

                // 오른쪽
                if (x + 1 < width && tiles[x + 1, y] != null &&
                    tiles[x + 1, y].tileType != TileType.Negative &&
                    tiles[x + 1, y].GetTileLayer() == tileLayer)
                {
                    tile.canGoRight = true;
                }
                else tile.canGoRight = false;
            }
        }
    }
}
