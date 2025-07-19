using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    [SerializeField]
    Character character;
    Animator animator;
    SpriteRenderer spriteRenderer;
    TurnManager turnManager;
    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        turnManager = FindObjectOfType<TurnManager>();
    }
    public void PlayAttack()
    {
        if (character.attackTargetCharacter.currentTile.gridPos.x <= character.currentTile.gridPos.x)
            spriteRenderer.flipX = true;
        else
            spriteRenderer.flipX = false;
        animator.SetTrigger("Attack");
        turnManager.isActing = true;
    }
    public void PlayHit()
    {
        animator.SetTrigger("Hit");
    }
    public void PlayIdle()
    {
        animator.SetTrigger("Idle");
    }
    public void PlayRunning()
    {
        animator.SetTrigger("Run");
    }
    public void PlayDie()
    {
        animator.SetTrigger("Die");
    }
}
