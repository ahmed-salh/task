using UnityEngine;
using System.Collections;

/// <summary>
/// Controls a 2x2 grid of colored squares that can expand and shrink.
/// When one square expands, the others shrink proportionally to maintain the grid layout.
/// Supports resetting back to equal division of space.
/// </summary>
public class QuadScaler : MonoBehaviour
{
    #region Inspector Fields

    [Header("Squares")]
    [Tooltip("Top-left square in the 2x2 grid")]
    public RectTransform red;

    [Tooltip("Top-right square in the 2x2 grid")]
    public RectTransform green;

    [Tooltip("Bottom-left square in the 2x2 grid")]
    public RectTransform blue;

    [Tooltip("Bottom-right square in the 2x2 grid")]
    public RectTransform yellow;

    [Header("Initial Layout")]
    [Range(0.1f, 0.9f)]
    [Tooltip("What percentage of the container each square takes up initially (0.5 = 50% = equal division)")]
    private readonly float initialDivision = 0.5f;

    [Header("Behavior")]
    [Range(0.5f, 0.95f)]
    [Tooltip("How much of the container the expanded square should take up (0.9 = 90%)")]
    public float expandedRatio = 0.9f;

    [Tooltip("Duration of the animation in seconds (e.g., 1.0 = one second)")]
    public float animationDuration = 1f;

    #endregion

    #region Private Fields

    // Reference to this GameObject's RectTransform (the container holding all squares)
    private RectTransform container;

    // Reference to the currently running animation coroutine
    private Coroutine animationRoutine;

    // Starting sizes for each square when animation begins
    private Vector2 redStart;
    private Vector2 greenStart;
    private Vector2 blueStart;
    private Vector2 yellowStart;

    // Target sizes for each square (what size they're animating towards)
    private Vector2 redTarget;
    private Vector2 greenTarget;
    private Vector2 blueTarget;
    private Vector2 yellowTarget;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Initialize component references when the script wakes up
    /// </summary>
    void Awake()
    {
        container = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Set initial square sizes when the script starts
    /// </summary>
    void Start()
    {
        // Initialize all squares to their starting sizes
        ResetToInitial();
    }

    /// <summary>
    /// Check for keyboard input each frame to trigger square expansion
    /// </summary>
    void Update()
    {
        // R key expands the red (top-left) square
        if (Input.GetKeyDown(KeyCode.R)) ExpandRed();

        // G key expands the green (top-right) square
        if (Input.GetKeyDown(KeyCode.G)) ExpandGreen();

        // B key expands the blue (bottom-left) square
        if (Input.GetKeyDown(KeyCode.B)) ExpandBlue();

        // Y key expands the yellow (bottom-right) square
        if (Input.GetKeyDown(KeyCode.Y)) ExpandYellow();

        // Space key resets all squares to equal division
        if (Input.GetKeyDown(KeyCode.Space)) ResetToInitial();
    }

    #endregion

    #region Size Calculation Properties

    /// <summary>
    /// Gets the width of the container (assumes square container)
    /// </summary>
    private float ContainerSize => container.rect.width;

    /// <summary>
    /// Calculates the size an expanded square should be based on the expandedRatio
    /// Example: If container is 100px and ratio is 0.9, this returns 90px
    /// </summary>
    private float ExpandedSize => ContainerSize * expandedRatio;

    /// <summary>
    /// Calculates the size of shrunken squares (the remaining space after expansion)
    /// Example: If container is 100px and expanded is 90px, this returns 10px
    /// </summary>
    private float ShrunkSize => ContainerSize - ExpandedSize;

    /// <summary>
    /// Calculates the initial size for each square based on the initialDivision ratio
    /// Example: If container is 100px and initialDivision is 0.5, this returns 50px
    /// </summary>
    private float InitialSize => ContainerSize * initialDivision;

    /// <summary>
    /// Helper method to create a Vector2 for size dimensions
    /// </summary>
    /// <param name="width">Width of the square</param>
    /// <param name="height">Height of the square</param>
    /// <returns>Vector2 representing the size</returns>
    private Vector2 Size(float width, float height)
    {
        return new Vector2(width, height);
    }

    #endregion

    #region Expansion Methods

    /// <summary>
    /// Resets all squares to their initial equal division
    /// Press SPACE to activate this in Play mode
    /// Layout: [HALF][HALF]
    ///         [HALF][HALF]
    /// </summary>
    private void ResetToInitial()
    {
        float size = InitialSize;

        SetTargets(
            red: Size(size, size),       // Equal width, equal height
            green: Size(size, size),     // Equal width, equal height
            blue: Size(size, size),      // Equal width, equal height
            yellow: Size(size, size)     // Equal width, equal height
        );
    }

    /// <summary>
    /// Expands the red square (top-left) while shrinking the others
    /// Layout: [LARGE][small]
    ///         [LARGE][small]
    /// </summary>
    private void ExpandRed()
    {
        SetTargets(
            red: Size(ExpandedSize, ExpandedSize),      // Large width, large height
            green: Size(ShrunkSize, ExpandedSize),      // Small width, large height
            blue: Size(ExpandedSize, ShrunkSize),       // Large width, small height
            yellow: Size(ShrunkSize, ShrunkSize)        // Small width, small height
        );
    }

    /// <summary>
    /// Expands the green square (top-right) while shrinking the others
    /// Layout: [small][LARGE]
    ///         [small][LARGE]
    /// </summary>
    private void ExpandGreen()
    {
        SetTargets(
            red: Size(ShrunkSize, ExpandedSize),        // Small width, large height
            green: Size(ExpandedSize, ExpandedSize),    // Large width, large height
            blue: Size(ShrunkSize, ShrunkSize),         // Small width, small height
            yellow: Size(ExpandedSize, ShrunkSize)      // Large width, small height
        );
    }

    /// <summary>
    /// Expands the blue square (bottom-left) while shrinking the others
    /// Layout: [LARGE][small]
    ///         [LARGE][small]
    /// </summary>
    private void ExpandBlue()
    {
        SetTargets(
            red: Size(ExpandedSize, ShrunkSize),        // Large width, small height
            green: Size(ShrunkSize, ShrunkSize),        // Small width, small height
            blue: Size(ExpandedSize, ExpandedSize),     // Large width, large height
            yellow: Size(ShrunkSize, ExpandedSize)      // Small width, large height
        );
    }

    /// <summary>
    /// Expands the yellow square (bottom-right) while shrinking the others
    /// Layout: [small][LARGE]
    ///         [small][LARGE]
    /// </summary>
    private void ExpandYellow()
    {
        SetTargets(
            red: Size(ShrunkSize, ShrunkSize),          // Small width, small height
            green: Size(ExpandedSize, ShrunkSize),      // Large width, small height
            blue: Size(ShrunkSize, ExpandedSize),       // Small width, large height
            yellow: Size(ExpandedSize, ExpandedSize)    // Large width, large height
        );
    }

    #endregion

    #region Animation System

    /// <summary>
    /// Sets the target sizes for all squares and starts the animation
    /// </summary>
    /// <param name="red">Target size for red square</param>
    /// <param name="green">Target size for green square</param>
    /// <param name="blue">Target size for blue square</param>
    /// <param name="yellow">Target size for yellow square</param>
    private void SetTargets(Vector2 red, Vector2 green, Vector2 blue, Vector2 yellow)
    {
        // Store the current sizes as starting points for the animation
        redStart = this.red.sizeDelta;
        greenStart = this.green.sizeDelta;
        blueStart = this.blue.sizeDelta;
        yellowStart = this.yellow.sizeDelta;

        // Store the target sizes
        redTarget = red;
        greenTarget = green;
        blueTarget = blue;
        yellowTarget = yellow;

        // Stop any currently running animation to prevent conflicts
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        // Start the new animation
        animationRoutine = StartCoroutine(Animate());
    }

    /// <summary>
    /// Coroutine that smoothly animates all squares to their target sizes over the specified duration
    /// Uses a time-based approach for consistent animation speed regardless of distance
    /// </summary>
    private IEnumerator Animate()
    {
        // Track how much time has elapsed since the animation started
        float elapsedTime = 0f;

        // Continue animating until the full duration has passed
        while (elapsedTime < animationDuration)
        {
            // Increment elapsed time by the time since last frame
            elapsedTime += Time.deltaTime;

            // Calculate the progress percentage (0.0 to 1.0)
            // Clamped to ensure we don't go over 1.0 due to frame timing
            float t = Mathf.Clamp01(elapsedTime / animationDuration);

            // Animate all squares based on the progress percentage
            red.sizeDelta = Vector2.Lerp(redStart, redTarget, t);
            green.sizeDelta = Vector2.Lerp(greenStart, greenTarget, t);
            blue.sizeDelta = Vector2.Lerp(blueStart, blueTarget, t);
            yellow.sizeDelta = Vector2.Lerp(yellowStart, yellowTarget, t);

            // Wait for the next frame before continuing
            yield return null;
        }

        // Ensure all squares end up exactly at their target sizes
        // This prevents floating-point precision issues
        red.sizeDelta = redTarget;
        green.sizeDelta = greenTarget;
        blue.sizeDelta = blueTarget;
        yellow.sizeDelta = yellowTarget;
    }

    #endregion
}