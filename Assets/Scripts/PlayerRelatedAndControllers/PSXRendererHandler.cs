using UnityEngine;

public class PSXRendererHandler : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer RenderTexture;
    [SerializeField]
    private RenderTexture PSXRenderer;


    private Material sharedRenderTextureMaterial;

    public float FadeToWhite
    {
        get
        {
            return GetSharedRenderTextureMaterial().GetFloat("_FadeToWhite");
        }
        set
        {
            GetSharedRenderTextureMaterial().SetFloat("_FadeToWhite", value);
        }
    }

    public float AlternateTransistion
    {
        get
        {
            return GetSharedRenderTextureMaterial().GetFloat("_TransitionToAlternate");
        }
        set
        {
            GetSharedRenderTextureMaterial().SetFloat("_TransitionToAlternate", value);
        }
    }

    public float InterferenceAmount
    {
        get
        {
            return GetSharedRenderTextureMaterial().GetFloat("_InterferenceAmount");
        }
        set
        {
            GetSharedRenderTextureMaterial().SetFloat("_InterferenceAmount", value);
        }
    }

    public void ResetMat()
    {
        AlternateTransistion = 0;
        InterferenceAmount = 0;
        FadeToWhite = 0;
    }

    public void SetResolution(GrungeLevel grungeLevel) 
    {
        if (PSXRenderer != null)
        {
            int width;
            int height;

            if (grungeLevel.UseNative)
            {
                width = Screen.currentResolution.width;
                height = Screen.currentResolution.height;
            }
            else
            {
                width = grungeLevel.Width;
                height = grungeLevel.Height;
            }

            if (PSXRenderer.width != width || PSXRenderer.height != height)
            {
                PSXRenderer.Release();
                PSXRenderer.width = width;
                PSXRenderer.height = height;
                PSXRenderer.Create();
            }

        }
    }

    private Material GetSharedRenderTextureMaterial()
    {
        if (sharedRenderTextureMaterial != null) return sharedRenderTextureMaterial;
        else if (RenderTexture != null) sharedRenderTextureMaterial = RenderTexture.sharedMaterial;
        return sharedRenderTextureMaterial;
    }
}
