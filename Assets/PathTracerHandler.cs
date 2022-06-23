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
    private Matrix3x3[] pixels;
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
    [SerializeField] private bool showBVH;
    [Space]
    private ComputeBuffer pixelsCB;
    private MeshObj[] objs;
    private ComputeBuffer BVHCB;
    private List<BVHNode> BVH;
    private Sphere[] spheres;
    private Sphere.Data[] sphereDatas;
    ComputeBuffer spheresCB;    

    private List<Material> materials;
    private Material.Data[] materialDatas;
    private ComputeBuffer materialsCB;

    List<Texture2D> albedoMapList;
    Texture2DArray albedoMaps;

    List<Texture2D> normalMapList;
    Texture2DArray normalMaps;

    private void Awake() {
        previousFileName = fileName;

        pixels = new Matrix3x3[width * height];

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

        pixelsCB = new ComputeBuffer((int)(width * height), 9 * sizeof(float));
        pixelsCB.SetData(pixels);
        pathTracerCompute.SetBuffer(0, "pixels", pixelsCB);

        objs = FindObjectsOfType<MeshObj>();
        spheres = FindObjectsOfType<Sphere>();

        foreach (MeshObj obj in objs){
            obj.pathTracer = this;
            obj.mat.pathTracer = this;
        }
        foreach (Sphere sphere in spheres){
            sphere.pathTracer = this;
            sphere.mat.pathTracer = this;
        }

        List<RenderObject> renderObjs = new List<RenderObject>();
        renderObjs.AddRange(objs);
        renderObjs.AddRange(spheres);

        materials = new List<Material>();
        albedoMapList = new List<Texture2D>();
        normalMapList = new List<Texture2D>();

        foreach (RenderObject obj in renderObjs){
            int matIndex = materials.IndexOf(obj.mat);
            int albedoMapIndex = albedoMapList.IndexOf(obj.mat.albedoMap);
            int normalMapIndex = normalMapList.IndexOf(obj.mat.normalMap);

            if (matIndex == -1){
                materials.Add(obj.mat);
                matIndex = materials.Count - 1;
            }
            if (albedoMapIndex == -1 && obj.mat.albedoMap){
                albedoMapList.Add(obj.mat.albedoMap);
                albedoMapIndex = albedoMapList.Count - 1;
            }
            if (normalMapIndex == -1 && obj.mat.normalMap){
                normalMapList.Add(obj.mat.normalMap);
                normalMapIndex = normalMapList.Count - 1;
            }
            obj.mat.matIndex = matIndex;
            obj.mat.albedoMapIndex = albedoMapIndex;
            obj.mat.normalMapIndex = normalMapIndex;
        }

        materialDatas = new Material.Data[materials.Count];

        for (int i = 0; i < materials.Count; i++){
            materialDatas[i] = materials[i].GetData();
        }

        materialsCB = new ComputeBuffer(materialDatas.Length, 9 * sizeof(float) + 2 * sizeof(int));
        materialsCB.SetData(materialDatas);
        pathTracerCompute.SetBuffer(0, "materials", materialsCB);

        albedoMaps = new Texture2DArray(2048, 2048, albedoMapList.Count + 1, TextureFormat.RGBAFloat, 0, false);
        for (int i = 0; i < albedoMapList.Count; i++){
            albedoMaps.SetPixels(albedoMapList[i].GetPixels(), i, 0);
        }
        albedoMaps.Apply();
        pathTracerCompute.SetTexture(0, "_albedoMaps", albedoMaps);

        normalMaps = new Texture2DArray(2048, 2048, normalMapList.Count + 1, TextureFormat.RGBAFloat, 0, false);
        for (int i = 0; i < normalMapList.Count; i++){
            normalMaps.SetPixels(normalMapList[i].GetPixels(), i, 0);
        }
        normalMaps.Apply();
        pathTracerCompute.SetTexture(0, "_normalMaps", normalMaps);

        BVH = BVHNode.ConstructBVH(objs);

        BVHCB = new ComputeBuffer(BVH.Count, 30 * sizeof(float) + 4 * sizeof(int));
        BVHCB.SetData(BVH.ToArray());

        pathTracerCompute.SetBuffer(0, "BVH", BVHCB);
        pathTracerCompute.SetInt("BVHCount", BVH.Count);

        sphereDatas = new Sphere.Data[spheres.Length];
        spheresCB = new ComputeBuffer(spheres.Length + 1, 4 * sizeof(float) + sizeof(int));
        pathTracerCompute.SetBuffer(0, "spheres", spheresCB);
        pathTracerCompute.SetInt("sphereCount", spheres.Length);   
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

        for (int i = 0; i < materials.Count; i++){
            materialDatas[i] = materials[i].GetData();
        }
        materialsCB.SetData(materialDatas);

        for (int i = 0; i < spheres.Length; i++){
            sphereDatas[i] = spheres[i].GetData();
        }
        spheresCB.SetData(sphereDatas);

        pathTracerCompute.Dispatch(0, (int)width / 30, (int)height / 30, 1);
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
        
        ResetCurrSample();
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
        materialsCB.Release();
        BVHCB.Release();
        spheresCB.Release();
    }
    private void OnDrawGizmos() {
        if (!showBVH || BVH is null) return;
        Gizmos.color = Color.black;

        foreach (BVHNode node in BVH){
            Vector3 center = node.box.GetCenter();
            Gizmos.DrawWireCube(center, node.box.GetDims());
        }
    }
}
