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
    public TMP_Text spdText;
    public TMP_Text atkText;
    public TMP_Text magText;
    public TMP_Text defText;
    public TMP_Text criText;
    public TMP_Text agiText;
    public TMP_Text weaponText;
    public TMP_Text equipment1Text;
    public TMP_Text equipment2Text;
    public TMP_Text equipment3Text;

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
        spdText.text = $"{character.SPEED}";
        atkText.text = $"{character.ATK}";
        magText.text = $"{character.MAG}";
        defText.text = $"{character.DEF}";
        criText.text = $"{character.CRI}";
        agiText.text = $"{character.AGI}";
        weaponText.text = "None";
        equipment1Text.text = "None";
        equipment2Text.text = "None";
        equipment3Text.text = "None";
        int idx = 1;
        foreach(var equipment in character.Equipments)
        {
            if(equipment.equipmentType == EquipmentType.Weapon)
            {
                weaponText.text = $"{equipment.name}";
            }
            else
            {
                switch(idx)
                {
                    case 1:
                        equipment1Text.text = $"{equipment.name}";
                        idx++;
                        break;
                    case 2:
                        equipment2Text.text = $"{equipment.name}";
                        idx++;
                        break;
                    case 3:
                        equipment3Text.text = $"{equipment.name}";
                        break;
                }
            }
        }
    }

    public void HideCharacterInfo()
    {
        characterInfoPanel.SetActive(false);
    }
}
