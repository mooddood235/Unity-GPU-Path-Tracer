using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class Vector3Extensions
{
    public static Vector3 Rotate(this Vector3 v, Vector3 angles){
        angles *= Mathf.PI / 180f;

        angles *= -1;
        float sinX = Mathf.Sin(angles.x);
        float cosX = Mathf.Cos(angles.x); 

        float sinY = Mathf.Sin(angles.y);
        float cosY = Mathf.Cos(angles.y);

        float sinZ = Mathf.Sin(angles.z);
        float cosZ = Mathf.Cos(angles.z);

        float3x3 xRotator = new float3x3(
            1, 0, 0,
            0, cosX, -sinX,
            0, sinX, cosX
        );
        float3x3 yRotator = new float3x3(
            cosY, 0, sinY,
            0, 1, 0,
            -sinY, 0, cosY
        );
        float3x3 zRotator = new float3x3(
            cosZ, -sinZ, 0,
            sinZ, cosZ, 0,
            0, 0, 1
        );
        float3x3 generalRotator = math.mul(math.mul(zRotator, xRotator), yRotator);
        return math.mul(v, generalRotator);
    }

    public static Vector3 MultiplyComps(Vector3 v1, Vector3 v2){
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }
}
