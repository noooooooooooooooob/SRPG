using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using NUnit.Framework;
using UnityEngine;
using DG.Tweening;

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

    [Header("Skill")]
    public List<SkillData> skillDatas;

    public Vector2 spriteOffset;
    public Vector2Int gridPos;

    [Header("UI")]
    public GameObject actionPanel; // 캐릭터 전용 UI
    private CanvasGroup panelCanvasGroup; // UI 애니메이션용
    private Vector3 originalScale; // UI 애니메이션용
    public UnityEngine.UI.Image turnGaugeImage;
    public UnityEngine.UI.Image HPBarImage;
    public bool HasSelectedAction = false;
    [Header("Tiles")]
    private List<GridTile> movableTiles = new();
    private Dictionary<GridTile, GridTile> cameFrom = new();
    private List<GridTile> pathToTarget = new();
    public GridTile currentTile;
    void Start()
    {
        if (actionPanel != null)
        {
            // 원래 스케일 저장
            originalScale = actionPanel.transform.localScale;

            // CanvasGroup 확인
            panelCanvasGroup = actionPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
                panelCanvasGroup = actionPanel.AddComponent<CanvasGroup>();

            // 초기 상태: 안 보이게 설정 (스케일은 그대로!)
            panelCanvasGroup.alpha = 0f;
            actionPanel.SetActive(false);
        }
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

        skillDatas.Add(basicData.basicSkill);

        ResetGauge();
    }
    public void IncreaseGauge(float deltaTime)
    {
        turnGauge = Mathf.Min(100f, turnGauge + SPEED * deltaTime);
        turnGaugeImage.fillAmount = turnGauge / 100f;
    }

    public void ShowActionUI(bool show)
    {
        if (show)
        {
            actionPanel.SetActive(true);
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.blocksRaycasts = true; // 클릭 막기 활성화
            panelCanvasGroup.DOFade(1f, 0.3f);
            actionPanel.transform.localScale = originalScale * 0.8f;
            actionPanel.transform.DOScale(originalScale, 0.3f).SetEase(Ease.OutBack);
        }
        else
        {
            panelCanvasGroup.blocksRaycasts = false; // 클릭 막기 비활성화
            panelCanvasGroup.DOFade(0f, 0.2f);
            actionPanel.transform.DOScale(originalScale * 0.8f, 0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() => actionPanel.SetActive(false));
        }
    }
    public void SelectAction() // 버튼에 연결 -> 캐릭터의 행동 종료
    {
        HasSelectedAction = true;
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
        HPBarImage.fillAmount = (float)curHP / MaxHP;
    }
    public void Heal(int healAmount)
    {
        curHP = Mathf.Min(MaxHP, curHP + healAmount);
        HPBarImage.fillAmount = (float)curHP / MaxHP;
    }
    public void OnClickMove() // Move 클릭 시
    {
        ShowActionUI(false); // UI 닫기
        MapManager map = FindObjectOfType<MapManager>();
        ShowMovableTiles(map);
    }
    public void ShowMovableTiles(MapManager map)
    {
        ClearPreviousTiles();

        int range = AGI;
        Queue<(int x, int y, int dist)> queue = new();
        HashSet<GridTile> visited = new();

        int originX = currentTile.x;
        int originY = currentTile.y;

        GridTile[,] grid = map.grid; // 반드시 map에 grid 접근 가능해야 함
        int width = map.width;
        int height = map.height;

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

            if (visited.Contains(next))
                continue;

            if (next.currentCharacter == null) // 갈 수 있는 타일만
            {
                visited.Add(next);
                queue.Enqueue((nx, ny, 1));
            }
        }
        while (queue.Count > 0)
        {
            var (x, y, dist) = queue.Dequeue();
            if (dist > range) continue;

            GridTile tile = grid[x, y];
            movableTiles.Add(tile);
            tile.SetCanGo(true);
            tile.OnTileClicked = () => MoveTo(tile); // 클릭 시 이동

            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];

                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    continue;

                GridTile next = grid[nx, ny];

                if (visited.Contains(next))
                    continue;

                if (next.currentCharacter == null) // 갈 수 있는 타일만
                {
                    visited.Add(next);
                    queue.Enqueue((nx, ny, dist + 1));
                }
            }
        }
    }
    private void ClearPreviousTiles()
    {
        foreach (var tile in movableTiles)
        {
            tile.SetCanGo(false);
            tile.OnTileClicked = null;
        }
        movableTiles.Clear();
    }

    public void MoveTo(GridTile targetTile)
    {
        transform.position = targetTile.transform.position + (Vector3)spriteOffset;
        currentTile.currentCharacter = null; // 현재 타일의 캐릭터를 null로 설정
        currentTile = targetTile;
        targetTile.currentCharacter = this;

        ClearPreviousTiles();
        SelectAction();
    }
}
