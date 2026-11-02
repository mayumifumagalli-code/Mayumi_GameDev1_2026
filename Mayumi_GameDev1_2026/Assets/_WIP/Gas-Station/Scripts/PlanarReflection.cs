using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// A planar reflection for the wet forecourt, in edit mode too. A child camera mirrored below the plane renders the scene,
// fog included (its own volume: fog only, no grade), into _PlanarReflectionTex, which the wet-ground shader reads.
// The camera is a real one (not a flipped view matrix), so its image is the main view flipped vertically; the shader
// reads it at (u, 1 - v). The ground itself sits on the culling-excluded layer, so it doesn't hide the scene from below.
[ExecuteAlways]
public class PlanarReflection : MonoBehaviour
{
    public Camera source;
    public float planeHeight = 0f;
    [Range(0.25f, 1f)] public float resolutionScale = 0.5f;
    [Tooltip("What the reflection sees: not the ground (Water) or UI")] public LayerMask cullingMask = ~((1 << 4) | (1 << 5));
    [Tooltip("The volumes the reflection uses: its own, fog only")] public LayerMask volumeMask = 1 << 4;
    [Tooltip("Off: the reflection sees black where the sky is (HDRP's ray miss None), so the wet ground mirrors only the station and its haze")] public bool reflectSky = false;

    [SerializeField, HideInInspector] Camera reflection;
    RenderTexture target;
    static readonly int TexId = Shader.PropertyToID("_PlanarReflectionTex");

    public Camera ReflectionCamera { get { Setup(); return reflection; } }

    void OnEnable() { RenderPipelineManager.beginCameraRendering += OnBeginCamera; if (source) Mirror(); }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
        Shader.SetGlobalTexture(TexId, Texture2D.blackTexture);
        if (reflection) reflection.targetTexture = null;
        if (target) { target.Release(); DestroyImmediate(target); }
    }

    void Setup()
    {
        if (!reflection)
        {
            var t = transform.Find("Reflection Camera");
            var go = t ? t.gameObject : new GameObject("Reflection Camera");
            go.transform.SetParent(transform, false);
            reflection = go.GetComponent<Camera>();
            if (!reflection) reflection = go.AddComponent<Camera>();
        }
        reflection.cullingMask = cullingMask;
        reflection.clearFlags = reflectSky ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
        reflection.backgroundColor = Color.black;
        reflection.allowHDR = true;
        reflection.allowMSAA = false;
        reflection.allowDynamicResolution = false;
        reflection.depth = source ? source.depth - 1 : -2;
        var data = reflection.GetUniversalAdditionalCameraData();
        data.renderPostProcessing = true;          // the fog plugin runs only with post-processing on; the volume holds no grade
        data.antialiasing = AntialiasingMode.None;
        data.volumeLayerMask = volumeMask;
        data.volumeTrigger = null;
        data.renderShadows = true;
        data.requiresDepthOption = CameraOverrideOption.On;
    }

    void OnBeginCamera(ScriptableRenderContext context, Camera cam)
    {
        if (!source) return;
        if (cam == reflection) { Mirror(); Shader.SetGlobalTexture(TexId, Texture2D.blackTexture); return; }
        Shader.SetGlobalTexture(TexId, cam == source && target ? (Texture)target : Texture2D.blackTexture);
    }

    // The source camera mirrored in the plane, as a proper camera: forward mirrored, up mirrored and negated.
    void Mirror()
    {
        Setup();
        int w = Mathf.Max(16, Mathf.RoundToInt(source.pixelWidth * resolutionScale));
        int h = Mathf.Max(16, Mathf.RoundToInt(source.pixelHeight * resolutionScale));
        if (!target || target.width != w || target.height != h)
        {
            reflection.targetTexture = null;     // never release the camera's own target
            if (target) { target.Release(); DestroyImmediate(target); }
            target = new RenderTexture(w, h, 24, RenderTextureFormat.ARGBHalf) { name = "Planar Reflection", useMipMap = true, autoGenerateMips = true, hideFlags = HideFlags.DontSave };
            target.Create();
        }
        reflection.targetTexture = target;
        var p = source.transform.position; var f = source.transform.forward; var u = source.transform.up;
        reflection.transform.SetPositionAndRotation(
            new Vector3(p.x, 2f * planeHeight - p.y, p.z),
            Quaternion.LookRotation(new Vector3(f.x, -f.y, f.z), -new Vector3(u.x, -u.y, u.z)));
        reflection.fieldOfView = source.fieldOfView;
        reflection.aspect = source.aspect;
        reflection.nearClipPlane = source.nearClipPlane;
        reflection.farClipPlane = source.farClipPlane;
    }
}
