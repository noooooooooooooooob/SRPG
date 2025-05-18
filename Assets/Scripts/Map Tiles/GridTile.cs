using UnityEngine;

public class GridTile : MonoBehaviour
{
    [Header("타일 정보")]
    public int x;
    public int y;
    private SpriteRenderer spriteRenderer;
    public System.Action OnTileClicked;
    public Character currentCharacter;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetHighlight(bool on)
    {
        spriteRenderer.color = on ? Color.red : Color.white;
    }
    public void SetCanGo(bool on)
    {
        spriteRenderer.color = on ? Color.green : Color.white;
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
