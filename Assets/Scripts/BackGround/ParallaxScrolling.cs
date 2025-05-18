using UnityEngine;

public class ParallaxScrolling : MonoBehaviour
{
    public float parallaxSpeed;
    private float length;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;

        // Get the width of the background in world units
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            length = spriteRenderer.bounds.size.x;
        }
        else
        {
            Debug.LogWarning("SpriteRenderer component is missing on " + gameObject.name);
        }
    }

    private void Update()
    {
        // Move each background layer to the left based on parallaxSpeed
        transform.position += Vector3.left * parallaxSpeed * Time.deltaTime;

        // Reset the position to create an infinite scrolling effect
        if (transform.position.x < startPosition.x - length)
        {
            transform.position = startPosition;
        }
    }
}