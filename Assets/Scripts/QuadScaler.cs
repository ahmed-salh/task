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
    public float initialDivision = 0.5f;

    [Header("Behavior")]
    [Range(0.5f, 0.95f)]
    [Tooltip("How much of the container the expanded square should take up (0.9 = 90%)")]
    public float expandedRatio = 0.9f;

    [Tooltip("Speed of the size transition animation (higher = faster)")]
    public float animationSpeed = 8f;

    #endregion

    #region Private Fields

    // Reference to this GameObject's RectTransform (the container holding all squares)
    private RectTransform container;

    // Reference to the currently running animation coroutine
    private Coroutine animationRoutine;

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
    /// Coroutine that smoothly animates all squares to their target sizes
    /// Runs every frame until all squares reach their targets
    /// </summary>
    private IEnumerator Animate()
    {
        while (true)
        {
            // Track whether all squares have finished animating
            bool allSquaresFinished = true;

            // Animate each square and check if it's done
            // The &= operator combines the results (all must be true)
            allSquaresFinished &= AnimateSquare(red, redTarget);
            allSquaresFinished &= AnimateSquare(green, greenTarget);
            allSquaresFinished &= AnimateSquare(blue, blueTarget);
            allSquaresFinished &= AnimateSquare(yellow, yellowTarget);

            // If all squares are finished, exit the coroutine
            if (allSquaresFinished)
            {
                yield break;
            }

            // Wait for the next frame before continuing the animation
            yield return null;
        }
    }

    /// <summary>
    /// Animates a single square towards its target size using smooth interpolation
    /// </summary>
    /// <param name="square">The RectTransform to animate</param>
    /// <param name="targetSize">The size to animate towards</param>
    /// <returns>True if the square has reached its target (within 0.5 pixels), false otherwise</returns>
    private bool AnimateSquare(RectTransform square, Vector2 targetSize)
    {
        // Smoothly interpolate from current size to target size
        // Lerp creates smooth easing, with speed controlled by animationSpeed
        square.sizeDelta = Vector2.Lerp(
            square.sizeDelta,                    // Current size
            targetSize,                          // Target size
            Time.deltaTime * animationSpeed      // Interpolation factor (smooth over time)
        );

        // Check if we're close enough to the target (within 0.5 pixels)
        // This prevents infinite tiny movements and allows the animation to finish
        return Vector2.Distance(square.sizeDelta, targetSize) < 0.5f;
    }

    #endregion
}