using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshCollider))]
public abstract class RenderObject : MonoBehaviour
{
    [HideInInspector] public PathTracerHandler pathTracer;
    public Material mat;

    private void OnValidate() {
        if (pathTracer) pathTracer.ResetCurrSample();
    }
}
