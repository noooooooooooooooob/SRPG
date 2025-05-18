using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    public GameObject[] charactersToSpawn; // 생성할 캐릭터들
    public MapManager mapManager;             // 맵 참조
    public List<Character> spawnedCharacters = new();
    public void SpawnCharacters()
    {
        foreach (var prefab in charactersToSpawn)
        {
            Vector2Int pos = GetRandomWalkablePosition();
            GridTile tile = GetTile(pos);
            if (tile == null) continue;

            Vector3 tilePos = tile.transform.position;
            Vector3 spawnPos = new Vector3(tilePos.x, tilePos.y, tilePos.z - 0.01f);

            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);
            obj.name = prefab.name;
            Character character = obj.GetComponent<Character>();
            character.InitializeFromData();
            character.currentTile = tile;
            tile.currentCharacter = character;
            spawnedCharacters.Add(character);
        }
    }

    GridTile GetTile(Vector2Int pos)
    {
        return mapManager != null ? mapManager.GetTile(pos) : null;
    }

    Vector2Int GetRandomWalkablePosition()
    {
        for (int i = 0; i < 100; i++)
        {
            int x = Random.Range(0, mapManager.width);
            int y = Random.Range(0, mapManager.height);

            GridTile tile = GetTile(new Vector2Int(x, y));
            return new Vector2Int(x, y);
        }

        return new Vector2Int(0, 0); // fallback
    }
}
