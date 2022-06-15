using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
[RequireComponent(typeof(MeshFilter))]
public class MeshObj : MonoBehaviour
{
    [SerializeField] private Color albedo;
    [SerializeField] [Range(0f, 1f)] private float specChance;
    [SerializeField] [Range(0f, 1f)] private float metalness;
    [SerializeField] [Range(0f, 1f)] private float roughness;
    [SerializeField] private Vector3 emission;

    public Material GetMaterial(){
        return new Material{
            albedo = this.albedo,
            specChance = this.specChance,
            metalness = this.metalness,
            roughness = this.roughness,
            emission = this.emission
        };
    }
}
