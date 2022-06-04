using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere : MonoBehaviour
{
    [SerializeField] private PathTracerHandler pathTracerHandler;
    [SerializeField] private float radius;
    [SerializeField] private Color albedo;
    [SerializeField] [Range(0f, 1f)] private float specChance;
    [SerializeField] [Range(0f, 1f)] private float metalness;
    [SerializeField] [Range(0f, 1f)] private float roughness;
    [SerializeField] private Vector3 emission;
    private Vector3 previousPos;
    
    public Data GetData(){
        return new Data(transform.position, radius, new Vector3(albedo.r, albedo.g, albedo.b), specChance, metalness, roughness, emission);
    }
    private void Awake() {
        previousPos = transform.position;
    }
    private void Update() {
        if (transform.position != previousPos){
            pathTracerHandler.ResetCurrSample();
            previousPos = transform.position;
        }
    }
    private void OnValidate() {
        pathTracerHandler.ResetCurrSample();
    }
    public struct Data{
        private Vector3 center;
        private float radius;
        private Vector3 albedo;
        private float specChance;
        private float metalness;
        private float roughness;
        private Vector3 emission;

        public Data(Vector3 center, float radius, Vector3 albedo, float specChance, float metalness, float roughness, Vector3 emission){
            this.center = center;
            this.radius = radius;
            this.albedo = albedo;
            this.specChance = specChance;
            this.metalness = metalness;
            this.roughness = roughness;
            this.emission = emission;
        }
    }
}
