using UnityEngine;

public class Knight : Character
{
    void Awake()
    {
        InitializeFromData();
        // spriteOffset = new Vector2(0.243f,0.644f);
        transform.position += (Vector3)spriteOffset;
    }
}
