using UnityEngine;
using DG.Tweening;

public class GridTile : MonoBehaviour
{
    [Header("타일 정보")]
    private SpriteRenderer spriteRenderer;
    public System.Action OnTileClicked;
    public Character currentCharacter;
    public Vector2Int gridPos;
    bool isHighlighted = false; // 행동하고 있을 시
    public GameObject SetHighlightGameObject;
    private Tween highlightTween; // 하이라이트 애니메이션

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetHighlight(bool on)
    {
        if (!isHighlighted)
        {
            SetHighlightGameObject.SetActive(on);
            HighlightAnimation(on);
        }
    }
    void HighlightAnimation(bool on)
    {
        if (highlightTween != null && highlightTween.IsActive())
            highlightTween.Kill();

        if (on)
        {
            highlightTween = SetHighlightGameObject.transform
                .DOScale(1.5f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo); // 무한 반복
        }
        else
        {
            // 원래 크기로 복구하고 종료
            SetHighlightGameObject.transform.DOScale(1f, 0.2f);
        }
    }
    public void SetCanGo(bool on)
    {
        spriteRenderer.color = on ? Color.green : Color.white;
        isHighlighted = on;
    }
    public void SetCanAttack(bool on)
    {
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

}
