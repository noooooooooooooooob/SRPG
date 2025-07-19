using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.UI;

public class PriorityQueue<T>
{
    private List<(T item, int priority)> elements = new();

    public int Count => elements.Count;

    public void Enqueue(T item, int priority)
    {
        elements.Add((item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;
        for (int i = 1; i < elements.Count; i++)
        {
            if (elements[i].priority < elements[bestIndex].priority)
                bestIndex = i;
        }

        T bestItem = elements[bestIndex].item;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }

    public bool Contains(T item)
    {
        return elements.Any(e => EqualityComparer<T>.Default.Equals(e.item, item));
    }
}



public class Character : MonoBehaviour
{
    [Header("원본 데이터 참조")]
    public CharacterData basicData;
    [Header("STAT")]
    public int MaxHP;
    public int curHP;
    public int MaxSP;
    public int curSP;
    public int SPEED;
    public int ATK;
    public int ATKRange; // 공격범위
    public int MAG;
    public int DEF;
    public int AGI;
    public int CRI;
    public float turnGauge = 0f; // 0 ~ 100
    [Header("Equipment")]
    public List<WeaponData> Equipments = new();

    [Header("Skill")]
    public GameObject skillPanel;
    public List<Button> skillButtons;

    public Vector2 spriteOffset;

    [Header("UI")]
    public bool HasSelectedAction = false;
    [Header("Tiles")]
    private List<GridTile> movableTiles = new();
    private List<GridTile> attackableTiles = new();
    public GridTile currentTile;
    [Header("Components")]
    public Character attackTargetCharacter;
    TurnManager turnManager;
    MapManager mapManager;
    SpriteRenderer spriteRenderer;
    [SerializeField] CharacterAnimation characterAnimation;
    public CharacterUI characterUI;
    public bool isDie = false;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        foreach (var equipment in Equipments)
        {
            equipment.PlusStats(this);
        }
    }
    void OnEnable()
    {
        turnManager = FindObjectOfType<TurnManager>();
        mapManager = FindObjectOfType<MapManager>();
    }
    public void InitializeFromData()
    {
        if (basicData == null)
        {
            Debug.LogError("CharacterData가 연결되지 않았습니다.");
            return;
        }

        MaxHP = basicData.HP;
        curHP = basicData.HP;
        MaxSP = basicData.SP;
        curSP = basicData.SP;
        SPEED = basicData.SPEED;
        ATK = basicData.ATK;
        ATKRange = basicData.ATKRange;
        MAG = basicData.MAG;
        DEF = basicData.DEF;
        AGI = basicData.AGI;
        CRI = basicData.CRI;

        ResetGauge();
    }
    public void setLayer(string layerName)
    {
        SpriteRenderer[] spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (var spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sortingLayerName = layerName;
        }
    }
    public void IncreaseGauge(float amount)
    {
        turnGauge = Mathf.Min(100f, turnGauge + SPEED * amount);
        characterUI.SetGaugeImage(turnGauge);
    }
    public void SelectAction() // 버튼에 연결 -> 캐릭터의 행동 종료
    {
        HasSelectedAction = true;
        characterUI.ShowActionReturnUI(false);
        characterAnimation.PlayIdle();
        ResetGauge();
    }
    public void ResetGauge()
    {
        turnGauge = 0f;
        characterUI.SetGaugeImage(0);
    }
    public bool CanAct() => turnGauge >= 100f;

    public void TakeDamage(int damage)
    {
        curHP = Mathf.Max(0, curHP - damage);
        characterUI.SetHpBarImage((float)curHP / MaxHP);
        if (curHP <= 0)
        {
            isDie = true;
            turnManager.isActing = true;
            characterAnimation.PlayDie();
            ResetGauge();
            turnManager.removeDeadCharacter(this);
        }
    }
    public void DieCharacter()
    {
        turnManager.isActing = false;
        gameObject.SetActive(false);
    }
    public void Heal(int healAmount)
    {
        curHP = Mathf.Min(MaxHP, curHP + healAmount);
        characterUI.SetHpBarImage((float)curHP / MaxHP);
    }
    public void SPHeal(int healAmount)
    {
        curSP = Mathf.Min(MaxSP, curSP + healAmount);
    }
    // 여러 계산들
    public void ShowAttackableTiles()
    {
        ClearPreviousTiles();

        int range = ATKRange;
        Queue<(int x, int y, int dist)> queue = new();
        HashSet<GridTile> visited = new();

        int originX = currentTile.gridPos.x;
        int originY = currentTile.gridPos.y;

        GridTile[,] grid = mapManager.grid; // 반드시 map에 grid 접근 가능해야 함
        int width = mapManager.width;
        int height = mapManager.height;

        // 4방향
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };
        for (int i = 0; i < 4; i++)
        {
            int nx = originX + dx[i];
            int ny = originY + dy[i];

            if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                continue;

            GridTile next = grid[nx, ny];

            if (visited.Contains(next) || next.tileType == TileType.Negative)
                continue;

            visited.Add(next);
            queue.Enqueue((nx, ny, 1));
        }
        while (queue.Count > 0)
        {
            var (x, y, dist) = queue.Dequeue();
            if (dist > range) continue;

            GridTile tile = grid[x, y];
            attackableTiles.Add(tile);
            tile.SetCanAttack(true);
            tile.OnTileClicked = () => AttackTo(tile); // 클릭 시 공격

            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];

                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    continue;

                GridTile next = grid[nx, ny];

                if (visited.Contains(next) || next.tileType == TileType.Negative)
                    continue;

                visited.Add(next);
                queue.Enqueue((nx, ny, dist + 1));
            }
        }
    }
    public void ShowMovableTiles()
    {
        ClearPreviousTiles();

        int range = AGI;
        Queue<(GridTile tile, int dist)> queue = new();
        HashSet<GridTile> visited = new();

        queue.Enqueue((currentTile, 0));
        visited.Add(currentTile);

        while (queue.Count > 0)
        {
            var (tile, dist) = queue.Dequeue();
            if (dist > range) continue;

            if (tile != currentTile)
            {
                movableTiles.Add(tile);
                tile.SetCanGo(true);
                tile.OnTileClicked = () => MoveTo(tile);
            }

            foreach (var neighbor in GetNeighbors(tile))
            {
                if (neighbor.currentCharacter == null &&
                    neighbor.tileType != TileType.Negative &&
                    !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue((neighbor, dist + 1));
                }
            }
        }
    }
    public void ClearPreviousTiles() // 하이라이트 타일 전부 비우기
    {
        foreach (var tile in movableTiles)
        {
            tile.SetCanGo(false);
            tile.OnTileClicked = null;
        }
        foreach (var tile in attackableTiles)
        {
            tile.SetCanAttack(false);
            tile.OnTileClicked = null;
        }
        movableTiles.Clear();
    }
    IEnumerator MoveAlongPath(GridTile targetTile)
    {
        List<GridTile> path = FindPath(currentTile, targetTile);
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("경로가 없습니다.");
            yield break;
        }

        foreach (var tile in path)
        {
            Vector3 targetPos = tile.transform.position + (Vector3)spriteOffset;
            spriteRenderer.flipX = targetPos.x < transform.position.x;

            float duration = 0.2f;
            transform.DOMove(targetPos, duration).SetEase(Ease.Linear);
            yield return new WaitForSeconds(duration);

            currentTile.currentCharacter = null;
            currentTile = tile;
            currentTile.currentCharacter = this;
        }

        SelectAction();
    }
    private int Heuristic(GridTile a, GridTile b) // 맨해튼 거리
    {
        return Mathf.Abs(a.gridPos.x - b.gridPos.x) + Mathf.Abs(a.gridPos.y - b.gridPos.y);
    }
    private List<GridTile> ReconstructPath(Dictionary<GridTile, GridTile> cameFrom, GridTile current) // 경로 복원
    {
        List<GridTile> path = new List<GridTile> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }
    private List<GridTile> FindPath(GridTile start, GridTile goal)
    {
        GridTile[,] grid = mapManager.grid;
        int width = mapManager.width;
        int height = mapManager.height;

        var openSet = new PriorityQueue<GridTile>();
        var cameFrom = new Dictionary<GridTile, GridTile>();
        var gScore = new Dictionary<GridTile, int>();
        var fScore = new Dictionary<GridTile, int>();

        openSet.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (neighbor.currentCharacter != null && neighbor != goal) continue;
                if (neighbor.tileType == TileType.Negative) continue;

                int tentativeG = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);
                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }

        return null;
    }

    private IEnumerable<GridTile> GetNeighbors(GridTile tile)
    {
        int x = tile.gridPos.x;
        int y = tile.gridPos.y;
        int width = mapManager.width;
        int height = mapManager.height;
        GridTile[,] grid = mapManager.grid;

        // 위
        if (tile.canGoUp && y + 1 < height)
            yield return grid[x, y + 1];

        // 아래
        if (tile.canGoDown && y - 1 >= 0)
            yield return grid[x, y - 1];

        // 왼쪽
        if (tile.canGoLeft && x - 1 >= 0)
            yield return grid[x - 1, y];

        // 오른쪽
        if (tile.canGoRight && x + 1 < width)
            yield return grid[x + 1, y];
    }
    public void MoveTo(GridTile targetTile)
    {
        ClearPreviousTiles();
        characterAnimation.PlayRunning();
        StartCoroutine(MoveAlongPath(targetTile));
    }
    public void AttackTo(GridTile targetTile)
    {
        if (targetTile.currentCharacter == null) return;
        attackTargetCharacter = targetTile.currentCharacter;
        characterAnimation.PlayAttack();

        ClearPreviousTiles();
        SelectAction();
    }

    // Animations
    public void PlayTargetHitAnimation() // 공격 맞는 타이밍
    {
        attackTargetCharacter.characterAnimation.PlayHit();
    }
    public void AttackAnimationEnd()
    {
        turnManager.isActing = false;
        attackTargetCharacter.characterAnimation.PlayIdle();
        attackTargetCharacter.TakeDamage(ATK);
    }
}
