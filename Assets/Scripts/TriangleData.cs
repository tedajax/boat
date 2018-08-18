using UnityEngine;

public struct TriangleData
{
    public Vector3 p1 { get; private set; }
    public Vector3 p2 { get; private set; }
    public Vector3 p3 { get; private set; }

    public Vector3 center { get; private set; }

    public float distanceToSurface { get; private set; }

    public Vector3 normal { get; private set; }

    public float area { get; private set; }

    public TriangleData(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;

        center = (p1 + p2 + p3) / 3f;

        distanceToSurface = Mathf.Abs(WaterController.Instance.DistanceToWater(center, Time.time));

        normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;

        float a = Vector3.Distance(p1, p2);
        float c = Vector3.Distance(p1, p3);

        area = (a * c * Mathf.Sin(Vector3.Angle(p2 - p1, p3 - p1) * Mathf.Deg2Rad)) / 2f;
    }
}