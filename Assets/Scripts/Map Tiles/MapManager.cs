using UnityEngine;
using DG.Tweening;
using System.Collections;

public class MapManager : MonoBehaviour
{
    public CharacterSpawner characterSpawner;
    public Vector3 animStartPos = new Vector3(0, -20, 0);
    public GameObject tilePrefab;
    public int width;
    public int height;
    public float offset = -1.0f;

    public static bool isGenerating = true; // 전역에서 체크 가능하게 static

    public GridTile[,] grid;

    //부모 오브젝트 정리만을 위한 변수
    private Transform tileParent;

    public IEnumerator GenerateMapCoroutine()
    {
        isGenerating = true;
        GenerateMap();
        yield return new WaitUntil(() => isGenerating == false);
        yield return StartCoroutine(characterSpawner.SpawnCharactersWithEffect());
    }
    public void GenerateMap()
    {
        width = Random.Range(5, 15);
        height = Random.Range(5, 15);
        tileParent = new GameObject("TileContainer").transform;
        grid = new GridTile[width, height];
        float xoffset = 0.45f;
        float yoffset = 0.2f;
        int z = 0;

        float lastDelay = 0f;
        float delaytime = 0.1f;
        float dotweenTime = 0.5f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 finalPos = new Vector3(x * xoffset - xoffset * y + offset, x * yoffset + y * yoffset + offset, z++);
                Vector3 spawnPos = finalPos + animStartPos;

                GameObject obj = Instantiate(tilePrefab, spawnPos, Quaternion.identity, tileParent); // 부모 설정
                // obj.GetComponent<SpriteRenderer>().sortingOrder = z--;

                var tile = obj.GetComponent<GridTile>();
                tile.x = x;
                tile.y = y;
                grid[x, y] = tile;

                float delay = (x + y) * delaytime;
                lastDelay = Mathf.Max(lastDelay, delay);

                obj.transform.DOMoveY(finalPos.y, dotweenTime)
                    .SetEase(Ease.OutBack)
                    .SetDelay(delay);
            }
        }

        Invoke("EnableInput", lastDelay + 0.5f);
        
    }

    void EnableInput()
    {
        isGenerating = false;
        // characterSpawner.SpawnCharacters();
    }
    public GridTile GetTile(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x >= width || pos.y >= height)
            return null;
        return grid[pos.x, pos.y];
    }
}
