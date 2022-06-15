using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Triangle
{
    public Vector3 v0;
    public Vector3 v1;
    public Vector3 v2;
    public Triangle(Vector3 v0, Vector3 v1, Vector3 v2){
        this.v0 = v0;
        this.v1 = v1;
        this.v2 = v2;
    }   
   
}
