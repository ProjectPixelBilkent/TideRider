using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Turns off URP's runtime Rendering Debugger overlay so it can't be opened by accident on device.
/// </summary>
public static class DisableRenderingDebugUI
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Disable()
    {
        DebugManager.instance.enableRuntimeUI = false;
    }
}
