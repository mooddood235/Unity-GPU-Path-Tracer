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
    [SerializeField] private List<MeshObj> objs;
    private ComputeBuffer BVHCB;
    private List<BVHNode> debug;

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
    }
    private void Start() {
        debug = BVHNode.ConstructBVH(objs);
        List<BVHNode.Blittable> BVH = BVHNode.ConvertToBlittable(debug);

        BVHCB = new ComputeBuffer(BVH.Count, 24 * sizeof(float) + 3 * sizeof(int));
        BVHCB.SetData(BVH.ToArray());

        pathTracerCompute.SetBuffer(0, "BVH", BVHCB);
        pathTracerCompute.SetInt("BVHCount", BVH.Count);
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
        BVHCB.Release();
    }
    private void OnDrawGizmos() {
        if (!showBVH || debug is null) return;
        Gizmos.color = Color.black;

        foreach (BVHNode node in debug){
            Vector3 center = node.box.GetCenter();
            Gizmos.DrawWireCube(center, node.box.GetDims());
        }
    }
}
