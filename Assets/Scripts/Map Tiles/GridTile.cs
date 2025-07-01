using UnityEngine;
using DG.Tweening;

public enum TileType
{
    Positive,
    Negative,
    Stair
}

public class GridTile : MonoBehaviour
{
    [Header("타일 정보")]
    public TileType tileType;
    private SpriteRenderer spriteRenderer;
    public System.Action OnTileClicked;
    public Character currentCharacter;
    public Vector2Int gridPos;
    bool isHighlighted = false; // 행동하고 있을 시

    [Header("이동 가능 방향")]
    public bool canGoUp = true;
    public bool canGoDown = true;
    public bool canGoLeft = true;
    public bool canGoRight = true;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void SetHighlight(bool on)
    {
    }
    public void SetCanGo(bool on)
    {
        this.gameObject.SetActive(on);
    }
    public void SetCanAttack(bool on)
    {
        this.gameObject.SetActive(on);
        spriteRenderer.color = on ? Color.red : Color.white;
        isHighlighted = on;
    }
    void OnMouseDown()
    {
        OnTileClicked?.Invoke();
        if (currentCharacter != null)
        {
            UIManager.Instance.ShowCharacterInfo(currentCharacter);
        }
        else
        {
            UIManager.Instance.HideCharacterInfo();
        }
    }
    public int GetTileLayer()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError($"[GridTile] SpriteRenderer가 {gameObject.name}에 없습니다.");
            return -1;
        }

        string layerName = spriteRenderer.sortingLayerName;

        // 예: "Layer 0", "Layer 1"
        if (layerName.StartsWith("Layer "))
        {
            string numberPart = layerName.Substring("Layer ".Length);
            if (int.TryParse(numberPart, out int result))
                return result;
        }

        Debug.LogWarning($"Unknown sorting layer name: {layerName}");
        return -1; // 에러 처리용
    }
}
