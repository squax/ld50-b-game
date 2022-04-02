using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LD 50 - here a we a go!
/// </summary>
[ExecuteInEditMode]
public class LightingCameraEffect : MonoBehaviour
{
    [SerializeField]
    private LayerMask occlusionMask;

    [SerializeField]
    private LayerMask lightMask;

    [SerializeField]
    private LayerMask glowMask;

    [SerializeField]
    private Shader occlusionShader;

    [SerializeField]
    private Material renderEffectMaterial;

    private RenderTexture occlusionRT;
    private RenderTexture lightingRT;
    private RenderTexture glowRT;

    private Camera normalCamera;
    private Camera effectCamera;

    private Squax.Camera.FastBlur fastBlur;

    private void Awake()
    {
        normalCamera = GetComponent<UnityEngine.Camera>();
    }

    void OnPreCull()
    {
        if (occlusionRT != null && occlusionRT.IsCreated() == false)
        {
            // Release any temporary textures used.
            RenderTexture.ReleaseTemporary(occlusionRT);

            occlusionRT = null;
        }

        if (lightingRT != null && lightingRT.IsCreated() == false)
        {
            // Release any temporary textures used.
            RenderTexture.ReleaseTemporary(lightingRT);

            lightingRT = null;
        }

        if (glowRT != null && glowRT.IsCreated() == false)
        {
            // Release any temporary textures used.
            RenderTexture.ReleaseTemporary(glowRT);

            glowRT = null;
        }
        
        if (effectCamera == null)
        {
            // Create a temporary game object to house our effect camera.
            GameObject cameraGameObject = new GameObject("EffectCamera: " + gameObject.name);
            effectCamera = cameraGameObject.AddComponent<UnityEngine.Camera>();
            cameraGameObject.hideFlags = HideFlags.HideAndDontSave;

            // Fast blur incase we need it.
            fastBlur = cameraGameObject.AddComponent<Squax.Camera.FastBlur>();

            fastBlur.blurIterations = 1;
            fastBlur.blurSize = 1.6f;
            fastBlur.blurType = 0;
            fastBlur.downsample = 1;
        }

        fastBlur.enabled = false;

        effectCamera.CopyFrom(normalCamera);
        effectCamera.renderingPath = RenderingPath.Forward;
        effectCamera.enabled = false;
        effectCamera.backgroundColor = new Color(0, 0, 0, 0);
        effectCamera.clearFlags = CameraClearFlags.SolidColor;

        // Set the culling mask to only render objects with this layer.
        effectCamera.cullingMask = occlusionMask;

        // Create an appropriate render texture.
        if (occlusionRT == null)
        {
            int downSample = 1;
            occlusionRT = RenderTexture.GetTemporary(effectCamera.pixelWidth / downSample, effectCamera.pixelHeight / downSample, 0, RenderTextureFormat.DefaultHDR);
        }

        occlusionRT.filterMode = FilterMode.Point;

        //fastBlur.enabled = true;

        effectCamera.targetTexture = occlusionRT;
        effectCamera.RenderWithShader(occlusionShader, "RenderType");
        effectCamera.targetTexture = null;

        //fastBlur.enabled = false;

        effectCamera.CopyFrom(normalCamera);
        effectCamera.renderingPath = RenderingPath.Forward;
        effectCamera.enabled = false;
        effectCamera.backgroundColor = new Color(0, 0, 0, 0);
        effectCamera.clearFlags = CameraClearFlags.SolidColor;

        // Set the culling mask to only render objects with this layer.
        effectCamera.cullingMask = lightMask;

        // Create an appropriate render texture.
        if (lightingRT == null)
        {
            int downSample = 1;
            lightingRT = RenderTexture.GetTemporary(effectCamera.pixelWidth / downSample, effectCamera.pixelHeight / downSample, 0, RenderTextureFormat.DefaultHDR);
        }

        lightingRT.filterMode = FilterMode.Point;

        effectCamera.targetTexture = lightingRT;
        effectCamera.Render();
        effectCamera.targetTexture = null;

        effectCamera.CopyFrom(normalCamera);
        effectCamera.renderingPath = RenderingPath.Forward;
        effectCamera.enabled = false;
        effectCamera.backgroundColor = new Color(0, 0, 0, 0);
        effectCamera.clearFlags = CameraClearFlags.SolidColor;

        // Set the culling mask to only render objects with this layer.
        effectCamera.cullingMask = glowMask;

        // Create an appropriate render texture.
        if (glowRT == null)
        {
            int downSample = 1;
            glowRT = RenderTexture.GetTemporary(effectCamera.pixelWidth / downSample, effectCamera.pixelHeight / downSample, 0, RenderTextureFormat.DefaultHDR);
        }

        glowRT.filterMode = FilterMode.Point;

        effectCamera.targetTexture = glowRT;
        effectCamera.Render();
        effectCamera.targetTexture = null;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        renderEffectMaterial.SetTexture("_OcclusionSource", occlusionRT);
        renderEffectMaterial.SetTexture("_LightingSource", lightingRT);
        renderEffectMaterial.SetTexture("_GlowSource", glowRT);

        Graphics.Blit(source, destination, renderEffectMaterial, 0);
    }

    void ReleaseAll()
    {
        // Release any temporary textures used.
        RenderTexture.ReleaseTemporary(occlusionRT);
        RenderTexture.ReleaseTemporary(lightingRT);
        RenderTexture.ReleaseTemporary(glowRT);

        occlusionRT = null;
        lightingRT = null;
        glowRT = null;
    }
}
