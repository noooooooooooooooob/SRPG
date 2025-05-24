using UnityEngine;
using System.Collections.Generic;

public enum EquipmentType
{
    Weapon,
    Equipment,
    None
}

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public EquipmentType equipmentType;
    public string name;
    public List<WeaponEffect> effects;

    public int HP;
    public int SP;
    public int SPEED;
    public int ATK;
    public int ATKRange; // 공격범위
    public int MAG;
    public int DEF;
    public int AGI;
    public int CRI;
    public void ApplyEffect(Character user, Character target)
    {
        foreach (var effect in effects)
        {
            effect.ApplyEffect(user, target);
        }
    }
    public void PlusStats(Character character)
    {
        character.MaxHP += HP;
        character.curHP += HP;
        character.MaxSP += SP;
        character.curSP += SP;
        character.SPEED += SPEED;
        character.ATK += ATK;
        character.ATKRange += ATKRange;
        character.MAG += MAG;
        character.DEF += DEF;
        character.AGI += AGI;
        character.CRI += CRI;
    }
    public void MinusStats(Character character)
    {
        character.MaxHP -= HP;
        character.curHP -= HP;
        character.MaxSP -= SP;
        character.curSP -= SP;
        character.SPEED -= SPEED;
        character.ATK -= ATK;
        character.ATKRange -= ATKRange;
        character.MAG -= MAG;
        character.DEF -= DEF;
        character.AGI -= AGI;
        character.CRI -= CRI;
    }
}