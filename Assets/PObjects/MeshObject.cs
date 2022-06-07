using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

[RequireComponent(typeof(MeshFilter))]
public class MeshObject : PObject
{
    private Vector3[] verts;
    private int[] tris;

    private void Awake() {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        verts = mesh.vertices;
        tris = mesh.triangles;
    }
    public Data GetData(List<Vector3> verts, List<int> tris){
        int offset = verts.Count;
        int trisStart = tris.Count;
        verts.AddRange(this.verts);
        foreach (int tri in this.tris){
            tris.Add(tri + offset);
        }

        return new Data(
        transform.position, transform.localScale, transform.rotation.eulerAngles,
        trisStart, tris.Count - 1, albedo, specChance, metalness, roughness, emission);
    }
    public struct Data{
        Vector3 pos;
        Vector3 scale;
        Vector3 rotation;
        Vector3 albedo;
        float specChance;
        float metalness;
        float roughness;
        Vector3 emission;
        int trisStart;
        int trisEnd;

        public Data(Vector3 pos, Vector3 scale, Vector3 rotation, int trisStart, int trisEnd, Color albedo,
        float specChance, float metalness, float roughness, Vector3 emission){
            this.pos = pos;
            this.scale = scale;
            this.rotation = rotation;
            this.trisStart = trisStart;
            this.trisEnd = trisEnd;
            this.albedo = new Vector3(albedo.r, albedo.g, albedo.b);
            this.specChance = specChance;
            this.metalness = metalness;
            this.roughness = roughness;
            this.emission = emission;
        }
    }
}
