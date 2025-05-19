using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [Header("캐릭터 정보 패널")]
    public GameObject characterInfoPanel;
    public UnityEngine.UI.Image Portrait;
    public TMP_Text nameText;
    public TMP_Text hpText;
    public TMP_Text spText;

    void Awake()
    {
        Instance = this;
        characterInfoPanel.SetActive(false);
    }
    public void ShowCharacterInfo(Character character)
    {
        characterInfoPanel.SetActive(true);
        Portrait.sprite = character.basicData.portrait;
        nameText.text = character.name;
        hpText.text = $"HP: {character.curHP} / {character.MaxHP}";
        spText.text = $"SP: {character.curSP} / {character.MaxSP}";
    }

    public void HideCharacterInfo()
    {
        characterInfoPanel.SetActive(false);
    }
}
