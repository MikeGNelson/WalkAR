using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Rendering;

public class FOVSlider : MonoBehaviour
{

    
    public RenderTexture renderTexture;
    public bool IsLeft;
    public Renderer TestRenderer;

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
        RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
    }
    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_beginCameraRendering;
        RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
    }

    private void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        OnPreRender();
    }

    private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        //OnPostRender();
    }

    // Start is called before the first frame update
    void Start()
    {
        //camera = this.GetComponent<Camera>();
        //FOV = camera.fieldOfView;
        CreateRenderTexture();
    }

    private void Update()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //camera.fieldOfView = FOV;
    }

    void CreateRenderTexture()
    {
        renderTexture = new RenderTexture(XRSettings.eyeTextureDesc);
        GetComponent<Camera>().targetTexture = renderTexture;
    }

    private void OnPreRender()
    {
        Debug.Log("PreRender");
        UpdateWorldAndProjectionMatrices();
    }

    Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D texture = new Texture2D(512, 512, TextureFormat.RGB24, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        texture.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        texture.Apply();
        return texture;
    }

    private void UpdateWorldAndProjectionMatrices()
    {
        transform.position = Camera.main.transform.position;
        transform.rotation = Camera.main.transform.rotation;

        Camera camera = GetComponent<Camera>();

        // Depending on which camera we are, we need to setup our projection matrix
        // to match the eye we're intending to render
        //this.GetComponent<Camera>()
        if (IsLeft)
        {
            Debug.Log(Camera.main.GetStereoNonJitteredProjectionMatrix(Camera.StereoscopicEye.Left));
            //camera.projectionMatrix = camera.GetStereoNonJitteredProjectionMatrix(Camera.StereoscopicEye.Left) * Camera.main.GetStereoNonJitteredProjectionMatrix(Camera.StereoscopicEye.Left);
            camera.worldToCameraMatrix =  Camera.main.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
            TestRenderer.material.SetTexture("_BaseMap", (renderTexture));
        }
        else
        {
            //camera.projectionMatrix = Camera.main.GetStereoNonJitteredProjectionMatrix(Camera.StereoscopicEye.Right);
            camera.worldToCameraMatrix = Camera.main.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
            TestRenderer.material.SetTexture("_BaseMap", (renderTexture));
        }
        

        // Test to make it greyscale
        //RenderTexture.active = tex;
        //GL.Begin(GL.TRIANGLES);
        //GL.Clear(true, true, new Color(.5f, .5f, .5f, 1));
        //GL.End();
        //GetComponent<Camera>().targetTexture = tex;
    }

    
}
