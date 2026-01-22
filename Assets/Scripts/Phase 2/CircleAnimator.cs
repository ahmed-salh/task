using UnityEngine;
using UnityEngine.UI;

public class BouncingCircle : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 300f; // Speed in pixels per second

    [Header("Pulse Settings")]
    [SerializeField] private float pulseSpeed = 2f; // How fast it pulses
    [SerializeField] private float minScale = 0.8f; // Minimum scale
    [SerializeField] private float maxScale = 1.2f; // Maximum scale

    private RectTransform rectTransform;
    private Vector2 velocity;
    private Canvas canvas;
    private RectTransform canvasRect;
    private Vector2 baseSize; // Store original size for collision calculations

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();

        // Store the original size
        baseSize = rectTransform.sizeDelta;

        // Initialize velocity with random direction but constant magnitude
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
    }

    void Update()
    {
        // Update pulsing animation
        UpdatePulse();

        // Move the circle
        rectTransform.anchoredPosition += velocity * Time.deltaTime;

        // Check and handle boundary collisions
        CheckBoundaries();
    }

    void UpdatePulse()
    {
        // Calculate pulsing scale using sine wave
        float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
        rectTransform.localScale = Vector3.one * scale;
    }

    void CheckBoundaries()
    {
        Vector2 pos = rectTransform.anchoredPosition;

        // Get canvas boundaries
        float canvasWidth = canvasRect.sizeDelta.x;
        float canvasHeight = canvasRect.sizeDelta.y;

        // Calculate boundaries using current scaled size
        float currentScale = rectTransform.localScale.x;
        float radius = (baseSize.x * currentScale) / 2f; // Assuming circle is square
        float minX = -canvasWidth / 2f + radius;
        float maxX = canvasWidth / 2f - radius;
        float minY = -canvasHeight / 2f + radius;
        float maxY = canvasHeight / 2f - radius;

        bool bounced = false;

        // Check horizontal boundaries
        if (pos.x <= minX || pos.x >= maxX)
        {
            velocity.x = -velocity.x;
            // Clamp position to prevent getting stuck
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            bounced = true;
        }

        // Check vertical boundaries
        if (pos.y <= minY || pos.y >= maxY)
        {
            velocity.y = -velocity.y;
            // Clamp position to prevent getting stuck
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            bounced = true;
        }

        // Apply clamped position
        if (bounced)
        {
            rectTransform.anchoredPosition = pos;

            // Ensure velocity magnitude stays constant (prevent floating point drift)
            velocity = velocity.normalized * speed;
        }
    }
}