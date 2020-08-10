using UnityEngine;
using System.Collections.Generic;

public class BoatPhysics : MonoBehaviour
{
    public GameObject colliderObject;
    public GameObject underWaterObj;

    private ModifyBoatMesh modifyBoatMesh;

    private Mesh underWaterMesh;

    private Rigidbody boatRigidBody;

    // rho
    private float waterDensity = 1027f;

    private void Start()
    {
        boatRigidBody = GetComponent<Rigidbody>();

        modifyBoatMesh = new ModifyBoatMesh((colliderObject == null) ? gameObject : colliderObject);

        underWaterMesh = underWaterObj.GetComponent<MeshFilter>().mesh;
    }

    private void Update()
    {
        modifyBoatMesh.GenerateUnderwaterMesh();

        modifyBoatMesh.DisplayMesh(underWaterMesh, "UnderWater Mesh");
    }

    private void FixedUpdate()
    {
        if (modifyBoatMesh.underWaterTriangleData.Count > 0)
        {
            AddUnderWaterForces();
        }
    }

    private void AddUnderWaterForces()
    {
        foreach (TriangleData triangleData in modifyBoatMesh.underWaterTriangleData)
        {
            Vector3 buoyancyForce = BuoyancyForce(waterDensity, triangleData);

            boatRigidBody.AddForceAtPosition(buoyancyForce, triangleData.center);

            Debug.DrawRay(triangleData.center, triangleData.normal * 3f, Color.white);
            Debug.DrawRay(triangleData.center, buoyancyForce.normalized * -3f, Color.blue);
        }
    }

    private Vector3 BuoyancyForce(float waterDensity, TriangleData triangleData)
    {
        // buoyancy = rho * g * v
        // rho = density of the water
        // g = gravity
        // v = volume of fluid directly above curved surface
        // horizontal components are canceled out, only the vertical force remains.

        // V = z * S * n
        // z = distance to surface
        // S = surface area
        // n = normal to surface

        Vector3 fb = waterDensity *
            Physics.gravity.y *
            triangleData.distanceToSurface *
            triangleData.area *
            triangleData.normal;

        return Vector3.up * fb.y;
    }
}