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
    private RenderTexture normalsTexture;
    private RenderTexture albedoTexture;
    private Matrix4x4[] pixels;
    [SerializeField] private bool saveRender;
    [SerializeField] private bool saveNormals;
    [SerializeField] private bool saveAlbedo;
    [SerializeField] private string fileName;
    private string previousFileName;
    [Space]
    [SerializeField] private ComputeShader pathTracerCompute;
    [Space]
    [SerializeField] Texture enviromentTexture;
    [SerializeField] private Color enviromentColor;
    [SerializeField] private bool useColor;
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
    ComputeBuffer pixelsCB;

    private void Awake() {
        previousFileName = fileName;

        pixels = new Matrix4x4[width * height];

        currSample = 1;

        previousPos = transform.position;
        previousRot = transform.rotation;

        cam = GetComponent<Camera>();
        previousFocalLength = cam.focalLength;

        renderTexture = new RenderTexture((int)width, (int)height, 0);
        renderTexture.format = RenderTextureFormat.ARGBFloat;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.enableRandomWrite = true;

        normalsTexture = new RenderTexture((int)width, (int)height, 0);
        normalsTexture.format = RenderTextureFormat.ARGBFloat;
        normalsTexture.filterMode = FilterMode.Point;
        normalsTexture.enableRandomWrite = true;

        albedoTexture = new RenderTexture((int)width, (int)height, 0);
        albedoTexture.format = RenderTextureFormat.ARGBFloat;
        albedoTexture.filterMode = FilterMode.Point;
        albedoTexture.enableRandomWrite = true;

        pixelsCB = new ComputeBuffer((int)(width * height), 16 * sizeof(float));
        pixelsCB.SetData(pixels);
        pathTracerCompute.SetBuffer(0, "pixels", pixelsCB);
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
        pathTracerCompute.SetInt("samples", (int)samples);
        pathTracerCompute.SetTexture(0, "_enviromentTex", enviromentTexture);
        pathTracerCompute.SetVector("enviromentColor", new Vector3(enviromentColor.r, enviromentColor.g, enviromentColor.b));
        pathTracerCompute.SetBool("useColor", useColor);

        pathTracerCompute.SetTexture(0, "renderTex", renderTexture);
        pathTracerCompute.SetTexture(0, "normalsTex", normalsTexture);
        pathTracerCompute.SetTexture(0, "albedoTex", albedoTexture);

        ComputeBuffer spheresCB = new ComputeBuffer(spheres.Count + 1, 13 * sizeof(float));
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

        ComputeBuffer meshObjectDatasCB = new ComputeBuffer(meshObjects.Count + 1, 18 * sizeof(float) + 2 * sizeof(int));
        ComputeBuffer vertsCB = new ComputeBuffer(verts.Count + 1, 3 * sizeof(float));
        ComputeBuffer trisCB = new ComputeBuffer(tris.Count + 1, sizeof(int));

        meshObjectDatasCB.SetData(meshObjectDatas);
        vertsCB.SetData(verts);
        trisCB.SetData(tris);
        pathTracerCompute.SetBuffer(0, "meshObjects", meshObjectDatasCB);
        pathTracerCompute.SetBuffer(0, "verts", vertsCB);
        pathTracerCompute.SetBuffer(0, "tris", trisCB);
        pathTracerCompute.SetInt("meshObjectCount", meshObjects.Count);        

        pathTracerCompute.SetBuffer(0, "spheres", spheresCB);

        pathTracerCompute.Dispatch(0, (int)width / 30, (int)height / 30, 1);
        spheresCB.Release();
        meshObjectDatasCB.Release();
        vertsCB.Release();
        trisCB.Release();
    }

    public void ResetCurrSample(){
        currSample = 1;
    }
    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        if (saveRender || saveNormals || saveAlbedo){
            SaveRender();
            saveRender = false;
            saveNormals = false;
            saveAlbedo = false;
        }

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
        if (saveRender || saveNormals || saveAlbedo || fileName != previousFileName){
            previousFileName = fileName;
            return;
        }
        foreach (Sphere sphere in spheres){
            sphere.pathTracerHandler = this;
        }
        foreach (MeshObject meshObject in meshObjects){
            meshObject.pathTracerHandler = this;
        }
        ResetCurrSample();
    }

    private void GetMeshInfo(List<MeshObject.Data> meshObjectDatas, List<Vector3> verts, List<int> tris){
        foreach (MeshObject meshObject in meshObjects){
            meshObjectDatas.Add(meshObject.GetData(verts, tris));
        }
    }
    private void SaveRender(){
        if (fileName == "") return;

        RenderTexture.active = renderTexture;
        if (saveNormals) RenderTexture.active = normalsTexture;
        else if (saveAlbedo) RenderTexture.active = albedoTexture;

        Texture2D tex = new Texture2D((int)width, (int)height);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        RenderTexture.active = null;

        System.IO.File.WriteAllBytes(fileName, tex.EncodeToPNG());
    }

    private void OnApplicationQuit() {
        pixelsCB.Release();
    }
}
