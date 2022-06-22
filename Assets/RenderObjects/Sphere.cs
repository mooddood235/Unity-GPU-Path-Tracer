using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere : RenderObject
{
    private Vector3 prevPos;
    private Quaternion prevRot;
    private Vector3 prevScale;

    private void Awake() {
        prevPos = this.transform.position;
        prevRot = this.transform.rotation;
        prevScale = this.transform.localScale;
    }
    private void Update() {
        if (this.transform.position != prevPos || this.transform.rotation != prevRot || this.transform.localScale != prevScale){
            prevPos = this.transform.position;
            prevRot = this.transform.rotation;
            prevScale = this.transform.localScale;
            if (pathTracer) pathTracer.ResetCurrSample();
        }
    }
    public Data GetData(){
        return new Data(
        this.transform.position,
        Mathf.Max(Mathf.Max(this.transform.localScale.x, this.transform.localScale.y), this.transform.localScale.z) / 2f, this.mat.matIndex);
    }
    public struct Data{
        Vector3 pos;
        float radius;
        int matIndex;

        public Data(Vector3 pos, float radius, int matIndex){
            this.pos = pos;
            this.radius = radius;
            this.matIndex = matIndex;
        }
    }
}
