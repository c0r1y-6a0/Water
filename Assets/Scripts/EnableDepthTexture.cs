using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableDepthTexture : MonoBehaviour
{
    void Start()
    {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
    }
}
