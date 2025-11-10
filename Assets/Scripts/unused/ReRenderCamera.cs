using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ReRenderCamera : MonoBehaviour
{
    public GameObject LeftSource;
    public GameObject RightSource;
    public Material AlphaMaterial;
    public RenderTexture altTexture;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
        //RenderPipelineManager.endFrameRendering += RenderPipelineManager_endFrameRendering;
    }
    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_beginCameraRendering;
        //RenderPipelineManager.endFrameRendering -= RenderPipelineManager_endFrameRendering;
    }

    private void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        //OnPreRender();
    }

    private void RenderPipelineManager_endFrameRendering(RenderTexture source, RenderTexture destination)
    {
        //OnPostRender();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Draw the world
        Graphics.Blit(source, destination);
        Debug.Log("Render");
        if (Camera.main.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
        {
            Debug.Log("Left");
            RenderTexture leftTexture =  LeftSource.GetComponent<Camera>().targetTexture;
            Graphics.Blit(leftTexture, destination, AlphaMaterial);
        }
        else
        {
            Debug.Log("Right");
            RenderTexture rightTexture = RightSource.GetComponent<Camera>().targetTexture;
            Graphics.Blit(rightTexture, destination, AlphaMaterial);
        }
    }
}

