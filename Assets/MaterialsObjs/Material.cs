using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Material", menuName = "ScriptableObjects/Material")]
public class Material : ScriptableObject
{
    public Color albedo;
    [Range(0f, 1f)] public float specChance;
    [Range(0f, 1f)] public float metalness;
    [Range(0f, 1f)] public float roughness;
    public Vector3 emission;
    [HideInInspector] public int matIndex;
    [HideInInspector] public PathTracerHandler pathTracer;

    public Data GetData(){
        return new Data{
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

    public struct Data{
        public Vector3 albedo;
        public float specChance;
        public float metalness;
        public float roughness;
        public Vector3 emission;
    }
}
