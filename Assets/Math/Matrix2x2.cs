using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Matrix2x2
{
    public Vector3 row1;
    public Vector3 row2;

    public Matrix2x2(Vector3 row1, Vector3 row2){
        this.row1 = row1;
        this.row2 = row2;
    }
}
