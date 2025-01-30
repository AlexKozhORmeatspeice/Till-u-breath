using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUInstancingEnabler : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        MaterialPropertyBlock matProp = new MaterialPropertyBlock();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.SetPropertyBlock(matProp);
    }
}
