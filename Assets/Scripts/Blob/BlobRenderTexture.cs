using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

//That's claude work, i didn't do it 
public class BlobRenderTexture : MonoBehaviour
{
    [SerializeField] private Camera blobCamera;
    [SerializeField] private RawImage displayImage;
    [SerializeField] private int scaleDivider = 2;

    private RenderTexture _rt;

    void Start()
    {
        _rt = new RenderTexture(
            Screen.width / scaleDivider,
            Screen.height / scaleDivider,
            24,
            RenderTextureFormat.ARGB32
        );
        _rt.filterMode = FilterMode.Bilinear;
        _rt.Create();

        // Config URP obligatoire
        var cameraData = blobCamera.GetUniversalAdditionalCameraData();
        cameraData.renderType = CameraRenderType.Base;

        blobCamera.targetTexture = _rt;
        displayImage.texture = _rt;
    }

    void OnDestroy()
    {
        _rt.Release();
    }
}
