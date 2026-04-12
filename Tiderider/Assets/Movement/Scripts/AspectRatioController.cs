using UnityEngine;

/// <summary>
/// Attach to Main Camera in Movement scene.
/// Step 1: Run the game once and note the printed aspect ratio in the Console.
/// Step 2: Set DESIGN_ASPECT to that value (e.g. 0.4621f for 9:19.5).
/// The camera will then maintain the designed viewport on all devices.
/// </summary>
[RequireComponent(typeof(Camera))]
public class AspectRatioController : MonoBehaviour
{
    // ── Design constants ───────────────────────────────────────────────────────
    // Half-height of the camera as set in the scene (orthographic size = 9).
    public const float DESIGN_ORTHO_SIZE = 9f;

    // Width-to-height ratio of your design device.
    // Leave at 0 on first run — the Awake print will tell you the value to set.
    public const float DESIGN_ASPECT = 0.462054f;

    // ── Runtime read-only ──────────────────────────────────────────────────────
    // Designed half-width in world units. Available after Awake().
    public static float DesignedHalfWidth { get; private set; }
    // Active orthographic size after adjustment.
    public static float ActiveOrthoSize { get; private set; }

    // ──────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        Camera cam = GetComponent<Camera>();

        float deviceAspect = (float)Screen.width / Screen.height;

        // Always print so you can read the design ratio during development.
        Debug.Log(
            $"[AspectRatioController] Device aspect (width/height) = {deviceAspect:F6}  " +
            $"({Screen.width} x {Screen.height})  |  " +
            $"Set DESIGN_ASPECT to this value, then rebuild.");

        if (DESIGN_ASPECT > 0f)
        {
            // Ensure the full designed width is always visible.
            // On a narrower device the camera shows more vertical space (covered by your black sprite).
            float ratio = DESIGN_ASPECT / deviceAspect;
            cam.orthographicSize = DESIGN_ORTHO_SIZE * Mathf.Max(1f, ratio);
        }
        else
        {
            cam.orthographicSize = DESIGN_ORTHO_SIZE;
        }

        ActiveOrthoSize = cam.orthographicSize;
        DesignedHalfWidth = DESIGN_ORTHO_SIZE * (DESIGN_ASPECT > 0f ? DESIGN_ASPECT : deviceAspect);
    }
}
