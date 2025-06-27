using UnityEngine;

[CreateAssetMenu(fileName = "WeaponEffect", menuName = "Scriptable Objects/WeaponEffect")]
public abstract class WeaponEffect : ScriptableObject
{
    public abstract void ApplyEffect(Character user, Character target);
}
[CreateAssetMenu(menuName = "WeaponEffects/BonusDamageEffect")]
public class BonusDamageEffect : WeaponEffect
{
    public int bonusDamage;

    public override void ApplyEffect(Character user, Character target)
    {
        target.TakeDamage(bonusDamage);
    }
}