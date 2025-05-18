using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string Name;
    public Sprite portrait;
    [Header("전투 스탯")]
    public int HP;
    public int SP;
    public int SPEED;
    public int ATK;
    public int ATKRange; // 공격범위
    public int MAG;
    public int DEF;
    public int AGI;
    public int CRI;

    [Header("기본 스킬")]
    public SkillData basicSkill;

}