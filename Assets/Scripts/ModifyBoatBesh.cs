using UnityEngine;
using System.Collections.Generic;

public class ModifyBoatMesh
{
    private Transform boatTransform;
    Vector3[] boatVertices;
    int[] boatTriangles;

    public Vector3[] boatVerticesGlobal;
    float[] allDistancesToWater;

    public List<TriangleData> underWaterTriangleData = new List<TriangleData>();

    public ModifyBoatMesh(GameObject gameObject)
    {
        boatTransform = gameObject.transform;

        boatVertices = gameObject.GetComponent<MeshFilter>().mesh.vertices;
        boatTriangles = gameObject.GetComponent<MeshFilter>().mesh.triangles;

        boatVerticesGlobal = new Vector3[boatVertices.Length];
        allDistancesToWater = new float[boatVertices.Length];
    }

    public void GenerateUnderwaterMesh()
    {
        underWaterTriangleData.Clear();

        for (int i = 0; i < boatVertices.Length; ++i)
        {
            Vector3 globalPos = boatTransform.TransformPoint(boatVertices[i]);

            boatVerticesGlobal[i] = globalPos;

            allDistancesToWater[i] = WaterController.Instance.DistanceToWater(globalPos, Time.time);
        }

        AddTriangles();
    }

    private void AddTriangles()
    {
        List<VertexData> vertexData = new List<VertexData>();

        vertexData.Add(new VertexData());
        vertexData.Add(new VertexData());
        vertexData.Add(new VertexData());

        int i = 0;
        while (i < boatTriangles.Length)
        {
            for (int x = 0; x < 3; x++)
            {
                vertexData[x].distance = allDistancesToWater[boatTriangles[i]];
                vertexData[x].index = x;
                vertexData[x].globalVertexPos = boatVerticesGlobal[boatTriangles[i]];
                ++i;
            }

            if (vertexData[0].distance > 0f &&
                vertexData[1].distance > 0f &&
                vertexData[2].distance > 0f)
            {
                // all vertices are above the water so skip.
                continue;
            }

            if (vertexData[0].distance < 0f &&
                vertexData[1].distance < 0f &&
                vertexData[2].distance < 0f)
            {
                // all vertices underwater so just use the whole triangle.
                underWaterTriangleData.Add(new TriangleData(vertexData[0].globalVertexPos,
                    vertexData[1].globalVertexPos,
                    vertexData[2].globalVertexPos));
            }
            else
            {
                vertexData.Sort((x, y) => -x.distance.CompareTo(y.distance));

                if (vertexData[1].distance < 0f)
                {
                    // if the second vertex is underwater than only 1 is above the water.
                    AddTrianglesOneAboveWater(vertexData);
                }
                else
                {
                    // two must be above the water.
                    AddTrianglesTwoAboveWater(vertexData);
                }
            }
        }
    }

    private void AddTrianglesOneAboveWater(List<VertexData> vertexData)
    {
        Vector3 H = vertexData[0].globalVertexPos;

        // left of H is M, right of H is L
        Vector3 M = Vector3.zero;
        Vector3 L = Vector3.zero;

        int indexM = vertexData[0].index - 1;
        if (indexM < 0)
        {
            indexM = 2;
        }

        float distH = vertexData[0].distance;
        float distM = 0f;
        float distL = 0f;

        if (vertexData[1].index == indexM)
        {
            M = vertexData[1].globalVertexPos;
            L = vertexData[2].globalVertexPos;

            distM = vertexData[1].distance;
            distL = vertexData[2].distance;
        }
        else
        {
            M = vertexData[2].globalVertexPos;
            L = vertexData[1].globalVertexPos;

            distM = vertexData[2].distance;
            distL = vertexData[1].distance;
        }

        Vector3 MH = H - M;
        float Mt = -distM / (distH - distM);
        Vector3 primeM = Mt * MH + M;

        Vector3 LH = H - L;
        float Lt = -distL / (distH - distL);
        Vector3 primeL = Lt * LH + L;

        underWaterTriangleData.Add(new TriangleData(M, primeM, primeL));
        underWaterTriangleData.Add(new TriangleData(M, primeL, L));
    }

    private void AddTrianglesTwoAboveWater(List<VertexData> vertexData)
    {
        Vector3 L = vertexData[2].globalVertexPos;

        Vector3 H = Vector3.zero;
        Vector3 M = Vector3.zero;

        int indexH = vertexData[2].index + 1;
        if (indexH > 2)
        {
            indexH = 0;
        }

        float distL = vertexData[2].distance;
        float distH = 0f;
        float distM = 0f;

        if (vertexData[1].index == indexH)
        {
            H = vertexData[1].globalVertexPos;
            M = vertexData[0].globalVertexPos;

            distH = vertexData[1].distance;
            distM = vertexData[0].distance;
        }
        else
        {
            H = vertexData[0].globalVertexPos;
            M = vertexData[1].globalVertexPos;

            distH = vertexData[0].distance;
            distM = vertexData[1].distance;
        }

        Vector3 LM = M - L;
        float Lt = -distL / (distM - distL);
        Vector3 primeM = Lt * LM + L;

        Vector3 LH = H - L;
        float Ht = -distL / (distH - distL);
        Vector3 primeH = Ht * LH + L;

        underWaterTriangleData.Add(new TriangleData(L, primeH, primeM));
    }

    public void DisplayMesh(Mesh mesh, string name)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = 0; i < underWaterTriangleData.Count; ++i)
        {
            Vector3 p1 = boatTransform.InverseTransformPoint(underWaterTriangleData[i].p1);
            Vector3 p2 = boatTransform.InverseTransformPoint(underWaterTriangleData[i].p2);
            Vector3 p3 = boatTransform.InverseTransformPoint(underWaterTriangleData[i].p3);

            vertices.Add(p1);
            triangles.Add(vertices.Count - 1);
            vertices.Add(p2);
            triangles.Add(vertices.Count - 1);
            vertices.Add(p3);
            triangles.Add(vertices.Count - 1);
        }

        mesh.Clear();

        mesh.name = name;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateBounds();
    }

    public class VertexData
    {
        public float distance;
        public int index;
        public Vector3 globalVertexPos;
    }
}