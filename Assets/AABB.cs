using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AABB
{
    public Vector3 min;
    public Vector3 max;
    
    public AABB(Vector3 min, Vector3 max){
        this.min = min;
        this.max = max;
    }
    public AABB(Triangle triangle){
        this.min = new Vector3(
            Mathf.Min(Mathf.Min(triangle.v0.x, triangle.v1.x), triangle.v2.x),
            Mathf.Min(Mathf.Min(triangle.v0.y, triangle.v1.y), triangle.v2.y),
            Mathf.Min(Mathf.Min(triangle.v0.z, triangle.v1.z), triangle.v2.z)
        );
        this.min -= Vector3.one * 0.0001f;
        this.max = new Vector3(
            Mathf.Max(Mathf.Max(triangle.v0.x, triangle.v1.x), triangle.v2.x),
            Mathf.Max(Mathf.Max(triangle.v0.y, triangle.v1.y), triangle.v2.y),
            Mathf.Max(Mathf.Max(triangle.v0.z, triangle.v1.z), triangle.v2.z)
        );
        this.max += Vector3.one * 0.0001f;
    }
    public Vector3 GetDims(){
        return new Vector3(max.x - min.x, max.y - min.y, max.z - min.z);
    }
    public Vector3 GetCenter(){
        Vector3 dims = this.GetDims();

        return new Vector3(this.max.x - dims.x / 2, this.max.y - dims.y / 2, this.max.z - dims.z / 2);
    }
    
    public static AABB GetSurroundingBox(AABB box0, AABB box1){
        return new AABB(
            new Vector3(
                Mathf.Min(box0.min.x, box1.min.x),
                Mathf.Min(box0.min.y, box1.min.y),
                Mathf.Min(box0.min.z, box1.min.z)
            ),
            new Vector3(
                Mathf.Max(box0.max.x, box1.max.x),
                Mathf.Max(box0.max.y, box1.max.y),
                Mathf.Max(box0.max.z, box1.max.z)
            )
        );
    }
}
