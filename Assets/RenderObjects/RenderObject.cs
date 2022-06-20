using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RenderObject : MonoBehaviour
{
    [SerializeField] protected Color albedo;
    [SerializeField] [Range(0f, 1f)] protected float specChance;
    [SerializeField] [Range(0f, 1f)] protected float metalness;
    [SerializeField] [Range(0f, 1f)] protected float roughness;
    [SerializeField] protected Vector3 emission;

    [HideInInspector] public PathTracerHandler pathTracer;

    public Material GetMaterial(){
        return new Material{
            albedo = new Vector3(this.albedo.r, this.albedo.g, this.albedo.b),
            specChance = this.specChance,
            metalness = this.metalness,
            roughness = this.roughness,
            emission = this.emission
        };
    }
    private void OnValidate() {
        if (pathTracer) pathTracer.ResetCurrSample();
    }
}
