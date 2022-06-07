using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Camera))]
public class PathTracerHandler : MonoBehaviour
{
    float previousFocalLength;
    Vector3 previousPos;
    Quaternion previousRot;
    private Camera cam;
    private RenderTexture renderTexture;
    private Vector3[] pixels;
    [SerializeField] private ComputeShader pathTracerCompute;
    [SerializeField] Texture enviromentTexture;
    [Space]
    [SerializeField] private uint width;
    [SerializeField] private uint height;
    [Space]
    [SerializeField] private uint maxDepth;
    [SerializeField] private uint seed;
    [SerializeField] private uint samples;
    [SerializeField] private uint currSample;
    [Space]
    [SerializeField] private List<Sphere> spheres;
    [SerializeField] private List<MeshObject> meshObjects;
    private void Awake() {
        pixels = new Vector3[width * height];
        currSample = 1;
        previousPos = transform.position;
        previousRot = transform.rotation;
        cam = GetComponent<Camera>();
        previousFocalLength = cam.focalLength;
        renderTexture = new RenderTexture((int)width, (int)height, 0);
        renderTexture.format = RenderTextureFormat.ARGBFloat;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.enableRandomWrite = true;
    }
    private void Dispatch(){
        seed = (uint)Random.Range(200, 100000);
        pathTracerCompute.SetInt("texWidth", (int)width);
        pathTracerCompute.SetInt("texHeight", (int)height);
        pathTracerCompute.SetMatrix("cameraToWorld", cam.cameraToWorldMatrix);
        pathTracerCompute.SetMatrix("cameraInverseProjection", cam.projectionMatrix.inverse);
        pathTracerCompute.SetVector("camPos", transform.position);
        pathTracerCompute.SetInt("maxDepth", (int)maxDepth);
        pathTracerCompute.SetInt("seed", (int)seed);
        pathTracerCompute.SetInt("currSample", (int)currSample);
        pathTracerCompute.SetTexture(0, "_enviromentTex", enviromentTexture);

        ComputeBuffer pixelsCB = new ComputeBuffer((int)(width * height), 3 * sizeof(float));
        pixelsCB.SetData(pixels);
        pathTracerCompute.SetBuffer(0, "pixels", pixelsCB);

        pathTracerCompute.SetTexture(0, "tex", renderTexture);

        ComputeBuffer spheresCB = new ComputeBuffer(spheres.Count, 13 * sizeof(float));
        Sphere.Data[] sphereDatas = new Sphere.Data[spheres.Count];
        for (int i = 0; i < spheres.Count; i++){
            sphereDatas[i] = spheres[i].GetData();
        }
        spheresCB.SetData(sphereDatas);
        pathTracerCompute.SetInt("sphereCount", spheres.Count);

        List<MeshObject.Data> meshObjectDatas = new List<MeshObject.Data>();
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        GetMeshInfo(meshObjectDatas, verts, tris);

        ComputeBuffer meshObjectDatasCB = new ComputeBuffer(meshObjects.Count, 15 * sizeof(float) + 2 * sizeof(int));
        ComputeBuffer vertsCB = new ComputeBuffer(verts.Count, 3 * sizeof(float));
        ComputeBuffer trisCB = new ComputeBuffer(tris.Count, sizeof(int));

        meshObjectDatasCB.SetData(meshObjectDatas);
        vertsCB.SetData(verts);
        trisCB.SetData(tris);
        pathTracerCompute.SetBuffer(0, "meshObjects", meshObjectDatasCB);
        pathTracerCompute.SetBuffer(0, "verts", vertsCB);
        pathTracerCompute.SetBuffer(0, "tris", trisCB);
        pathTracerCompute.SetInt("meshObjectCount", meshObjects.Count);        

        pathTracerCompute.SetBuffer(0, "spheres", spheresCB);
        pathTracerCompute.Dispatch(0, (int)width / 30, (int)height / 30, 1);
        pixelsCB.GetData(pixels);
        pixelsCB.Dispose();
        spheresCB.Dispose();
        meshObjectDatasCB.Dispose();
        vertsCB.Dispose();
        trisCB.Dispose();
    }

    public void ResetCurrSample(){
        currSample = 1;
    }
    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        if (previousFocalLength != cam.focalLength || transform.position != previousPos || transform.rotation != previousRot){
            ResetCurrSample();
            previousFocalLength = cam.focalLength;
            previousPos = transform.position;
            previousRot = transform.rotation;
        }
        if (currSample > samples) return;
        Dispatch();
        Graphics.Blit(renderTexture, dest);
        currSample++;
    }
    private void OnValidate() {
        ResetCurrSample();
    }

    private void GetMeshInfo(List<MeshObject.Data> meshObjectDatas, List<Vector3> verts, List<int> tris){
        foreach (MeshObject meshObject in meshObjects){
            meshObjectDatas.Add(meshObject.GetData(verts, tris));
        }
    }
}
