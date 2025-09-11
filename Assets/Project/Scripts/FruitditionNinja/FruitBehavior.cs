using System.Collections;
using UnityEngine;

public enum FruitState
{
    Flying,     // Bay lên đỉnh
    Glowing,    // Đang glow và xoay
    Falling,    // Rơi xuống
    Sliced,     // Đã bị chém
    Destroyed   // Đã được thu hồi
}

[RequireComponent(typeof(Collider2D))]
public class FruitBehavior : MonoBehaviour
{
    [Header("References")]
    public GameObject slicedTopPiece;     // Mảnh trên (đã có sẵn trong prefab)
    public GameObject slicedBottomPiece;  // Mảnh dưới (đã có sẵn trong prefab)
    public ParticleSystem sliceEffect;    // Particle nổ ruột
    public AudioClip sliceSound;          // Âm thanh khi chém
    public FruitType fruitType;

    [Header("Movement Settings")]
    public float glowDuration = 1.3f;     // Thời gian glow
    public float glowRotateSpeed = 180f;  // Tốc độ xoay khi glow (degrees/sec)
    public float fallSpeed = 10f;         // Tốc độ rơi

    [Header("Visual Effects")]
    public float glowIntensity = 1.5f;    // Độ sáng khi glow
    public Color glowColor = Color.yellow;

    [Header("Sliced Physics")]
    public float sliceForce = 5f;         // Lực văng của 2 mảnh
    public float sliceLifetime = 2f;      // Thời gian tồn tại của mảnh

    [Header("Combo Integration")]
    [SerializeField] private bool showComboFeedback = true;
    [SerializeField] private Color comboValidColor = Color.green;
    [SerializeField] private Color comboInvalidColor = Color.red;

    public static event System.Action<FruitType, GameObject> OnFruitSliced;

    // Private variables
    private FruitState currentState = FruitState.Flying;
    private BeatNote beatNote;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float flightStartTime;
    private float glowStartTime;

    // Components
    private SpriteRenderer spriteRenderer;
    private Collider2D fruitCollider;
    private AudioSource audioSource;
    private FruitSpawner spawner;

    // Original values for reset
    private Color originalColor;
    private Vector3 originalScale;

    // Combo system integration
    private bool isPartOfActiveCombo = false;
    private int assignedComboId = -1;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        fruitCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Store original values
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        originalScale = transform.localScale;

        // Ensure pieces are hidden initially
        if (slicedTopPiece != null) slicedTopPiece.SetActive(false);
        if (slicedBottomPiece != null) slicedBottomPiece.SetActive(false);
    }

    public void Initialize(BeatNote note, FruitSpawner spawnerRef)
    {
        beatNote = note;
        spawner = spawnerRef;
        assignedComboId = note.comboId;

        // Reset state
        currentState = FruitState.Flying;
        transform.position = note.spawnPosition;
        transform.rotation = Quaternion.identity;
        isPartOfActiveCombo = true;

        // Reset visual
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
            spriteRenderer.enabled = true;
        }
        transform.localScale = originalScale;

        // Enable collider
        if (fruitCollider != null)
        {
            fruitCollider.enabled = true;
        }

        // Hide pieces
        if (slicedTopPiece != null) slicedTopPiece.SetActive(false);
        if (slicedBottomPiece != null) slicedBottomPiece.SetActive(false);

        // Set movement parameters
        startPosition = note.spawnPosition;
        targetPosition = note.peakPosition;
        flightStartTime = Time.time;

        StartCoroutine(FruitLifeCycle());
    }

    private IEnumerator FruitLifeCycle()
    {
        // Phase 1: Fly to peak (1 second)
        yield return StartCoroutine(FlyToPeak());

        // Phase 2: Glow and rotate (1.3 seconds)
        if (currentState == FruitState.Flying) // Chưa bị chém
        {
            yield return StartCoroutine(GlowAtPeak());
        }

        // Phase 3: Fall down if not sliced
        if (currentState == FruitState.Glowing) // Vẫn chưa bị chém
        {
            StartCoroutine(FallDown());
        }
    }

    private IEnumerator FlyToPeak()
    {
        currentState = FruitState.Flying;

        float duration = 1f; // 1 second as specified
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (currentState != FruitState.Flying) yield break; // Bị chém giữa chừng

            float t = elapsedTime / duration;

            // Use physics trajectory instead of simple lerp
            Vector3 currentPos = CalculateTrajectoryPosition(t);
            transform.position = currentPos;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure exact peak position
        transform.position = targetPosition;
    }

    private Vector3 CalculateTrajectoryPosition(float t)
    {
        // t: 0->1 representing flight progress
        Vector3 start = startPosition;
        Vector3 peak = targetPosition;

        // Linear interpolation for X
        float x = Mathf.Lerp(start.x, peak.x, t);

        // Parabolic trajectory for Y (considering gravity)
        float deltaY = peak.y - start.y;
        float gravity = 9.81f;
        float timeToReachPeak = 1f;

        // y = y0 + vy0*t - 0.5*g*t^2
        float currentTime = t * timeToReachPeak;
        float initialVelocityY = (deltaY + 0.5f * gravity * timeToReachPeak * timeToReachPeak) / timeToReachPeak;
        float y = start.y + initialVelocityY * currentTime - 0.5f * gravity * currentTime * currentTime;

        return new Vector3(x, y, start.z);
    }

    private IEnumerator GlowAtPeak()
    {
        currentState = FruitState.Glowing;
        glowStartTime = Time.time;

        // Start glow effects
        StartGlowEffects();

        float elapsedTime = 0f;
        while (elapsedTime < glowDuration)
        {
            if (currentState != FruitState.Glowing) yield break; // Bị chém

            // Rotate
            transform.Rotate(0, 0, glowRotateSpeed * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Stop glow effects
        StopGlowEffects();
    }

    private void StartGlowEffects()
    {
        if (spriteRenderer != null)
        {
            // Enhanced glow effect with combo awareness
            Color targetGlow = glowColor;

            // Add combo-specific visual hints
            if (isPartOfActiveCombo && showComboFeedback)
            {
                if (IsValidForCurrentCombo())
                {
                    targetGlow = Color.Lerp(glowColor, comboValidColor, 0.3f);
                }
            }

            // Tween to glow color and scale
            LeanTween.color(gameObject, targetGlow, 0.2f);
            LeanTween.scale(gameObject, originalScale * glowIntensity, 0.2f)
                     .setEaseOutBack();
        }
    }

    private void StopGlowEffects()
    {
        if (spriteRenderer != null)
        {
            // Tween back to normal
            LeanTween.color(gameObject, originalColor, 0.2f);
            LeanTween.scale(gameObject, originalScale, 0.2f);
        }
    }

    private IEnumerator FallDown()
    {
        currentState = FruitState.Falling;

        while (transform.position.y > -50f) // Fall until off screen
        {
            if (currentState != FruitState.Falling) yield break; // Bị chém

            Vector3 pos = transform.position;
            pos.y -= fallSpeed * Time.deltaTime;
            transform.position = pos;

            yield return null;
        }

        // Return to pool when off screen
        ReturnToPool();
    }

    public void SliceFruit()
    {
        if (currentState == FruitState.Sliced || currentState == FruitState.Destroyed)
            return;

        // Check if object is active
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("Trying to slice inactive fruit");
            return;
        }

        currentState = FruitState.Sliced;
        OnFruitSliced?.Invoke(fruitType, gameObject);

        // Stop all movement coroutines
        StopAllCoroutines();

        // Play slice effects
        PlaySliceEffects();

        // Create sliced pieces
        CreateSlicedPieces();

        // Hide original fruit sprite and collider (but keep gameObject active)
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        if (fruitCollider != null)
        {
            fruitCollider.enabled = false;
        }

        // Return to pool after effect duration - USE SAFE METHOD
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(ReturnAfterDelay(sliceLifetime));
        }
        else
        {
            // If somehow inactive, return immediately
            ReturnToPool();
        }
    }

    public static void SimulateSlice(FruitType type, GameObject dummy)
    {
        OnFruitSliced?.Invoke(type, dummy);
    }

    private bool CheckComboValidity()
    {
        if (!isPartOfActiveCombo) return false;

        // Check with combo manager if this fruit type is currently valid
        var comboManager = ComboPanelManager.Instance;
        if (comboManager == null) return false;

        // This would be determined by the combo system
        // For now, assume it's valid if part of active combo
        return true;
    }

    private bool IsValidForCurrentCombo()
    {
        // Check if this fruit type is currently needed for any active combo
        var comboManager = ComboPanelManager.Instance;
        if (comboManager == null) return false;

        // This could be expanded to check actual combo state
        return isPartOfActiveCombo;
    }

    private void PlaySliceEffects(bool isValidForCombo = true)
    {
        // Enhanced particle effect based on combo validity
        if (sliceEffect != null)
        {
            sliceEffect.transform.position = transform.position;

            // Modify particle color for combo feedback
            if (showComboFeedback)
            {
                var main = sliceEffect.main;
                main.startColor = isValidForCombo ? comboValidColor : comboInvalidColor;
            }

            sliceEffect.Play();
        }

        // Play sound with pitch variation for combo feedback
        if (audioSource != null && sliceSound != null)
        {
            audioSource.pitch = isValidForCombo ? 1.2f : 0.8f;
            audioSource.PlayOneShot(sliceSound);
            audioSource.pitch = 1f; // Reset pitch
        }

        // Screen shake for valid combo hits
        if (isValidForCombo)
        {
            // Could add camera shake here
            // CameraShake.Instance?.Shake(0.1f, 0.2f);
        }
    }

    private void CreateSlicedPieces()
    {
        // Determine slice direction based on fruit type
        Vector2 topDirection, bottomDirection;
        GetSliceDirections(out topDirection, out bottomDirection);

        // Enable and launch top piece
        if (slicedTopPiece != null)
        {
            slicedTopPiece.transform.position = transform.position;
            slicedTopPiece.transform.rotation = transform.rotation;
            slicedTopPiece.SetActive(true);
            LaunchSlicedPiece(slicedTopPiece, topDirection);
        }

        // Enable and launch bottom piece  
        if (slicedBottomPiece != null)
        {
            slicedBottomPiece.transform.position = transform.position;
            slicedBottomPiece.transform.rotation = transform.rotation;
            slicedBottomPiece.SetActive(true);
            LaunchSlicedPiece(slicedBottomPiece, bottomDirection);
        }
    }

    private void GetSliceDirections(out Vector2 topDirection, out Vector2 bottomDirection)
    {
        switch (fruitType)
        {
            case FruitType.Banana:
            case FruitType.Grape:
                // Banana and Grape: left-right split
                topDirection = Vector2.left;
                bottomDirection = Vector2.right;
                break;

            default:
                // Other fruits: top-bottom split
                topDirection = Vector2.up;
                bottomDirection = Vector2.down;
                break;
        }
    }

    private void LaunchSlicedPiece(GameObject piece, Vector2 direction)
    {
        // Check if this object is still active before starting coroutine
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("Trying to launch sliced piece on inactive fruit object");
            return;
        }

        // Add random angle variation
        float angleVariation = Random.Range(-30f, 30f);
        direction = Quaternion.Euler(0, 0, angleVariation) * direction;

        // Add Rigidbody2D for physics
        Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = piece.AddComponent<Rigidbody2D>();
        }

        // Launch with force
        rb.AddForce(direction * sliceForce, ForceMode2D.Impulse);

        // Add slight rotation
        rb.angularVelocity = Random.Range(-360f, 360f);

        // Hide piece after lifetime - USE STATIC COROUTINE METHOD
        FruitSliceCoroutineRunner.Instance.StartDelayedAction(sliceLifetime, () => {
            HidePieceCallback(piece);
        });
    }

    private IEnumerator HidePieceAfterDelay(GameObject piece, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (piece != null)
        {
            piece.SetActive(false);

            // Reset rigidbody
            var rb = piece.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
    }

    private IEnumerator ReturnAfterDelay(float delay)
    {
        float elapsed = 0f;
        while (elapsed < delay)
        {
            // Check if object is still active and valid
            if (!gameObject.activeInHierarchy || currentState == FruitState.Destroyed)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        ReturnToPool();
    }

    private void HidePieceCallback(GameObject piece)
    {
        if (piece != null && piece.activeInHierarchy)
        {
            piece.SetActive(false);

            // Reset rigidbody
            var rb = piece.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
    }

    private void ReturnToPool()
    {
        // Prevent multiple returns
        if (currentState == FruitState.Destroyed) return;

        currentState = FruitState.Destroyed;
        isPartOfActiveCombo = false;

        // Stop all tweens on this object
        LeanTween.cancel(gameObject);

        // Reset visual properties
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
        }
        transform.localScale = originalScale;
        transform.rotation = Quaternion.identity;

        // Re-enable collider
        if (fruitCollider != null)
        {
            fruitCollider.enabled = true;
        }

        // Hide sliced pieces
        if (slicedTopPiece != null) slicedTopPiece.SetActive(false);
        if (slicedBottomPiece != null) slicedBottomPiece.SetActive(false);

        // Return to spawner pool
        if (spawner != null)
        {
            spawner.ReturnToPool(beatNote.fruitType, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // Test methods
    public void TestSlice()
    {
        SliceFruit();
    }

    // Public methods for game systems
    public bool CanBeSliced()
    {
        return currentState == FruitState.Flying ||
               currentState == FruitState.Glowing ||
               currentState == FruitState.Falling;
    }

    public FruitState GetCurrentState() => currentState;
    public FruitType GetFruitType() => fruitType;
    public int GetComboId() => assignedComboId;

    public float GetGlowProgress()
    {
        if (currentState != FruitState.Glowing) return -1f;

        float elapsedTime = Time.time - glowStartTime;
        return Mathf.Clamp01(elapsedTime / glowDuration);
    }

    // Mouse and keyboard input for testing
    void Update()
    {
        // Test input - remove in production
        if (Input.GetKeyDown(KeyCode.B))
        {
            TestSlice();
        }
    }

    void OnMouseDown()
    {
        // Alternative way to slice - click on fruit
        if (CanBeSliced())
        {
            SliceFruit();
        }
    }
}