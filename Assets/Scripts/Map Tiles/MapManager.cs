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

    public Transform mapRoot; // 맵 오브젝트 참조
    public GridTile[,] grid;

    void Start()
    {
        BuildGridFromHierarchy();
    }
    public IEnumerator GenerateMapCoroutine()
    {
        isGenerating = true;
        GenerateMap();
        yield return new WaitUntil(() => isGenerating == false);
        yield return StartCoroutine(characterSpawner.SpawnCharactersWithEffect());
    }
    void BuildGridFromHierarchy()
    {
        height = mapRoot.childCount; // 필드에 직접 대입
        width = mapRoot.GetChild(0).childCount;

        grid = new GridTile[width, height];

        for (int y = 0; y < height; y++)
        {
            Transform row = mapRoot.GetChild(y);
            for (int x = 0; x < row.childCount; x++)
            {
                GridTile tile = row.GetChild(x).GetComponent<GridTile>();
                tile.gameObject.SetActive(false);
                grid[x, y] = tile;

                tile.gridPos = new Vector2Int(x, y);
            }
        }
    }
    public void GenerateMap()
    {
        BuildGridFromHierarchy();

        float lastDelay = 0f;
        float delaytime = 0.1f;
        float dotweenTime = 0.5f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var tile = grid[x, y].GetComponent<GridTile>();
                tile.gameObject.SetActive(true);

                Vector3 finalPos = tile.transform.position;
                Vector3 spawnPos = finalPos + animStartPos;
                tile.transform.position = spawnPos;

                float delay = (x + y) * delaytime;
                lastDelay = Mathf.Max(lastDelay, delay);

                tile.transform.DOMoveY(finalPos.y, dotweenTime)
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
