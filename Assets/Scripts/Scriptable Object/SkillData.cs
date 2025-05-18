using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
public class SkillData : ScriptableObject
{
    [Header("스킬 기본 정보")]
    public string Name;
    public float ATKrate;
    public float MAGrate;
    public string description;
    public int cost;
    public int range;
}
