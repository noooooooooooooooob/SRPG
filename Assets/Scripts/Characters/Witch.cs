using UnityEngine;

public class Witch : Character
{
    void Awake()
    {
        InitializeFromData();
        transform.position += (Vector3)spriteOffset;
    }
}
