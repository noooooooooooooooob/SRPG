using UnityEngine;

public class GridTile : MonoBehaviour
{
    [Header("타일 정보")]
    public int x;
    public int y;
    private SpriteRenderer spriteRenderer;
    public System.Action OnTileClicked;
    public Character currentCharacter;
    bool isHighlighted = false; // 행동하고 있을 시

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetHighlight(bool on)
    {
        if(!isHighlighted)
            spriteRenderer.color = on ? Color.blue : Color.white;
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
