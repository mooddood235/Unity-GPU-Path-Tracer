using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))] 
public class PObject : MonoBehaviour
{
    [SerializeField] protected PathTracerHandler pathTracerHandler;
    [SerializeField] protected Color albedo;
    [SerializeField] [Range(0f, 1f)] protected float specChance;
    [SerializeField] [Range(0f, 1f)] protected float metalness;
    [SerializeField] [Range(0f, 1f)] protected float roughness;
    [SerializeField] protected Vector3 emission;
    protected Vector3 previousPos;
    protected Vector3 previousScale;

    private void Awake() {
        previousPos = transform.position;
    }
    private void Update() {
        if (transform.position != previousPos || transform.localScale != previousScale){
            pathTracerHandler.ResetCurrSample();
            previousPos = transform.position;
            previousScale = transform.localScale;
        }
    }
    private void OnValidate() {
        pathTracerHandler.ResetCurrSample();
    }
}
