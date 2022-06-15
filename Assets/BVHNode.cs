using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BVHNode
{
    int isBox;

    Triangle triangle;

    public AABB box;

    int left;
    int right;

    Material mat;

    public static List<BVHNode> ConstructBVH(List<MeshObj> objs){
        List<BVHNode> triangles = new List<BVHNode>();
        
        foreach (MeshObj obj in objs){
            Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
            Vector3[] verts = mesh.vertices;
            int[] tris = mesh.triangles;

            for (int i = 0; i < tris.Length; i+=3){
                Vector3 objRotation = obj.transform.root.eulerAngles;
                Vector3 objScale = obj.transform.localScale;
                Vector3 objPos = obj.transform.position;

                Vector3 v0 = 
                Vector3Extensions.MultiplyComps(verts[tris[i]].Rotate(objRotation), objScale) + objPos;
                Vector3 v1 = 
                Vector3Extensions.MultiplyComps(verts[tris[i+1]].Rotate(objRotation), objScale) + objPos;
                Vector3 v2 = 
                Vector3Extensions.MultiplyComps(verts[tris[i+2]].Rotate(objRotation), objScale) + objPos;

                triangles.Add(new BVHNode(new Triangle(v0, v1, v2), obj.GetMaterial())); 
            }
        }
        
        List<BVHNode> BVH = new List<BVHNode>();
        ConstructBVH(BVH, triangles, 0, triangles.Count);
        return BVH;
    }
    static private int ConstructBVH(List<BVHNode> BVH, List<BVHNode> triangles, int start, int end){
        int count = end - start;
        int thisIndex = BVH.Count;
        if (count < 1) return -1;

        else if (count == 1){
            BVH.Add(triangles[start]);
            return thisIndex;
        }

        AABB box = triangles[start].box;

        for (int i = start + 1; i < end; i++){
            box = AABB.GetSurroundingBox(box, triangles[i].box);
        }

        Vector3 dims = box.GetDims();
        float maxDim = Mathf.Max(Mathf.Max(dims.x, dims.y), dims.z);

        Comparer comparer;

        if (maxDim == dims.x) comparer = new Comparer(0);
        else if (maxDim == dims.y) comparer = new Comparer(1);
        else comparer = new Comparer(2);

        triangles.Sort(start, count, comparer);

        BVH.Add(new BVHNode(box));

        int mid = start + count / 2;

        BVH[thisIndex].left = ConstructBVH(BVH, triangles, start, mid);
        BVH[thisIndex].right = ConstructBVH(BVH, triangles, mid, end);

        return thisIndex;
    }

    private BVHNode(Triangle triangle, Material mat){
        this.isBox = 0;
        this.triangle = triangle;
        this.box = new AABB(triangle);
        this.left = -1;
        this.right = -1;
        this.mat = mat;
    }
    private BVHNode(AABB box){
        this.isBox = 1;
        this.box = box;
        this.left = -1;
        this.right = -1;
    }

    public static List<Blittable> ConvertToBlittable(List<BVHNode> nodes){
        List<Blittable> blittableNodes = new List<Blittable>(nodes.Count);

        foreach (BVHNode node in nodes){
            blittableNodes.Add(new Blittable(node));
        }

        return blittableNodes;
    }

    public struct Blittable{
        int isBox;

        Vector3 v0;
        Vector3 v1;
        Vector3 v2;

        Vector3 min;
        Vector3 max;

        int left;
        int right;

        Vector3 albedo;
        float specChance;
        float metalness;
        float roughness;
        Vector3 emission;

        public Blittable(BVHNode node){
            this.isBox = node.isBox;

            this.v0 = node.triangle.v0;
            this.v1 = node.triangle.v1;
            this.v2 = node.triangle.v2;

            this.min = node.box.min;
            this.max = node.box.max;

            this.left = node.left;
            this.right = node.right;

            this.albedo = new Vector3(node.mat.albedo.r, node.mat.albedo.g, node.mat.albedo.b);
            this.specChance = node.mat.specChance;
            this.metalness = node.mat.metalness;
            this.roughness = node.mat.roughness;
            this.emission = node.mat.emission;
        }
    }

    private struct Comparer : IComparer<BVHNode>{
        private int axis;

        public int Compare(BVHNode node0, BVHNode node1){
            if (node0.box.min[axis] == node1.box.min[axis]) return 0;
            else if (node0.box.min[axis] < node1.box.min[axis]) return -1;
            return 1;
        }

        public Comparer(int axis){
            this.axis = axis;
        }
    }
    

}
