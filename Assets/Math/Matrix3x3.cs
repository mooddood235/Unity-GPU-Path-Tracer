using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Matrix3x3
{
    public Vector3 row1;
    public Vector3 row2;
    public Vector3 row3;

    public Matrix3x3(Vector3 row1, Vector3 row2, Vector3 row3){
        this.row1 = row1;
        this.row2 = row2;
        this.row3 = row3;
    }
}
