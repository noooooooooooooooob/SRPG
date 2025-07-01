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
    public List<SkillData> skillDatas = new();
    public GameObject skillPanel;
    public List<Button> skillButtons;

    public Vector2 spriteOffset;

    [Header("UI")]
    public GameObject actionPanel; // 캐릭터 전용 UI
    public GameObject actionPanelReturnPanel; // 행동 취소
    private CanvasGroup actionPanelCanvasGroup; // UI 애니메이션용
    private CanvasGroup actionPanelReturnPanelCanvasGroup; // UI 애니메이션용
    private Vector3 originalScale; // UI 애니메이션용
    private Vector2 originalAnchoredPos; // UI 애니메이션용
    public UnityEngine.UI.Image turnGaugeImage;
    public UnityEngine.UI.Image HPBarImage;
    public bool HasSelectedAction = false;
    [Header("Tiles")]
    private List<GridTile> movableTiles = new();
    private List<GridTile> attackableTiles = new();
    public GridTile currentTile;
    [Header("Animation")]
    Animator animator;
    Character attackTargetCharacter;
    public bool isDie = false;
    TurnManager turnManager;
    MapManager mapManager;
    SpriteRenderer spriteRenderer;
    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        foreach (var equipment in Equipments)
        {
            equipment.PlusStats(this);
        }
        if (actionPanel != null)
        {
            // 원래 스케일 저장
            originalScale = actionPanel.transform.localScale;

            // CanvasGroup 확인
            actionPanelCanvasGroup = actionPanel.GetComponent<CanvasGroup>();
            if (actionPanelCanvasGroup == null)
                actionPanelCanvasGroup = actionPanel.AddComponent<CanvasGroup>();

            // 초기 상태: 안 보이게 설정
            actionPanelCanvasGroup.alpha = 0f;
            actionPanel.SetActive(false);

            // CanvasGroup 확인
            actionPanelReturnPanelCanvasGroup = actionPanelReturnPanel.GetComponent<CanvasGroup>();
            if (actionPanelReturnPanelCanvasGroup == null)
                actionPanelReturnPanelCanvasGroup = actionPanelReturnPanel.AddComponent<CanvasGroup>();

            // 초기 상태: 안 보이게 설정
            actionPanelReturnPanelCanvasGroup.alpha = 0f;
            actionPanelReturnPanel.SetActive(false);
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
        turnGaugeImage.fillAmount = turnGauge / 100f;
    }

    public void ShowActionUI(bool show)
    {
        originalAnchoredPos = actionPanel.GetComponent<RectTransform>().anchoredPosition;
        RectTransform rect = actionPanel.GetComponent<RectTransform>();
        Vector2 offScreenPos = originalAnchoredPos + new Vector2(-500f, 0f); // 왼쪽으로 이동

        if (show)
        {
            actionPanel.SetActive(true);
            actionPanelCanvasGroup.alpha = 0f;
            actionPanelCanvasGroup.blocksRaycasts = true;

            rect.anchoredPosition = offScreenPos;
            rect.DOAnchorPos(originalAnchoredPos, 0.3f).SetEase(Ease.OutCubic);
            actionPanelCanvasGroup.DOFade(1f, 0.3f);
        }
        else
        {
            actionPanelCanvasGroup.blocksRaycasts = false;

            rect.DOAnchorPos(offScreenPos, 0.2f).SetEase(Ease.InCubic);
            actionPanelCanvasGroup.DOFade(0f, 0.2f)
                .OnComplete(() => actionPanel.SetActive(false));
        }
    }
    public void ShowActionReturnUI(bool show)
    {
        originalAnchoredPos = actionPanelReturnPanel.GetComponent<RectTransform>().anchoredPosition;

        RectTransform rect = actionPanelReturnPanel.GetComponent<RectTransform>();
        Vector2 offScreenPos = originalAnchoredPos + new Vector2(-500f, 0f); // 왼쪽 오프스크린 위치

        if (show)
        {
            actionPanelReturnPanel.SetActive(true);
            actionPanelReturnPanelCanvasGroup.alpha = 0f;
            actionPanelReturnPanelCanvasGroup.blocksRaycasts = true;

            rect.anchoredPosition = offScreenPos; // 위치 초기화
            rect.DOAnchorPos(originalAnchoredPos, 0.3f).SetEase(Ease.OutCubic); // 슬라이드 인
            actionPanelReturnPanelCanvasGroup.DOFade(1f, 0.3f); // 페이드 인
        }
        else
        {
            actionPanelReturnPanelCanvasGroup.blocksRaycasts = false;

            rect.DOAnchorPos(offScreenPos, 0.2f).SetEase(Ease.InCubic); // 슬라이드 아웃
            actionPanelReturnPanelCanvasGroup.DOFade(0f, 0.2f) // 페이드 아웃
                .OnComplete(() => actionPanelReturnPanel.SetActive(false));
        }
    }
    public void SelectAction() // 버튼에 연결 -> 캐릭터의 행동 종료
    {
        HasSelectedAction = true;
        ShowActionReturnUI(false);
        animator.SetTrigger("Idle");
        ResetGauge();
    }
    public void ResetGauge()
    {
        turnGauge = 0f;
        turnGaugeImage.fillAmount = turnGauge / 100f;
    }
    public bool CanAct() => turnGauge >= 100f;

    public void TakeDamage(int damage)
    {
        curHP = Mathf.Max(0, curHP - damage);
        HPBarImage.DOFillAmount((float)curHP / MaxHP, 0.3f);
        if (curHP <= 0)
        {
            isDie = true;
            turnManager.isActing = true;
            animator.SetTrigger("Die");
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
        HPBarImage.DOFillAmount((float)curHP / MaxHP, 0.3f);
    }
    public void SPHeal(int healAmount)
    {
        curSP = Mathf.Min(MaxSP, curSP + healAmount);
    }
    public void OnClickMove() // Move 클릭 시
    {
        ShowActionUI(false); // UI 닫기
        ShowActionReturnUI(true);
        ShowMovableTiles();
    }
    public void OnClickAttack() // Attack 클릭 시
    {
        ShowActionUI(false); // UI 닫기
        ShowActionReturnUI(true);
        ShowAttackableTiles();
    }
    public void OnClickSkill() // Skill 클릭 시
    {
        ShowActionUI(false); // UI 닫기
        showSkillUI(true);
        ShowActionReturnUI(true);
    }
    void showSkillUI(bool show)
    {
        skillPanel.SetActive(show);
    }
    public void OnClickRest() // Rest 클릭 시
    {
        ShowActionUI(false); // UI 닫기
        SPHeal(30);
        ResetGauge();
    }
    public void OnClickReturn() // 행동 취소 시
    {
        ShowActionReturnUI(false);
        showSkillUI(false);
        ClearPreviousTiles();
        ShowActionUI(true);
    }
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

                if (visited.Contains(next))
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
    private void ClearPreviousTiles() // 하이라이트 타일 전부 비우기
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

    public void MoveTo(GridTile targetTile)
    {
        ClearPreviousTiles();
        PlayRunning();
        StartCoroutine(MoveAlongPath(targetTile));
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

    public void AttackTo(GridTile targetTile)
    {
        if (targetTile.currentCharacter == null) return;
        attackTargetCharacter = targetTile.currentCharacter;
        PlayAttackAnimation();

        ClearPreviousTiles();
        SelectAction();
    }

    // Animations
    public void PlayHitAnimation()
    {
        animator.SetTrigger("Hit");
    }
    public void PlayTargetHitAnimation() // 공격 맞는 타이밍
    {
        attackTargetCharacter.PlayHitAnimation();
    }
    public void PlayAttackAnimation()
    {
        if (attackTargetCharacter.currentTile.gridPos.x <= currentTile.gridPos.x)
            spriteRenderer.flipX = true;
        else
            spriteRenderer.flipX = false;
        animator.SetTrigger("Attack");
        turnManager.isActing = true;
    }
    public void AnimationEnd()
    {
        turnManager.isActing = false;
        attackTargetCharacter.PlayIdleAnimation();
        attackTargetCharacter.TakeDamage(ATK);
    }
    public void PlayIdleAnimation()
    {
        animator.SetTrigger("Idle");
    }
    public void PlayRunning()
    {
        animator.SetTrigger("Run");
    }
}
