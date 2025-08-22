using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleInstance : MonoBehaviour
{
    public List<MeshRenderer> meshes;

    public void ApplyMaterial(Material mat)
    {
        foreach (var mesh in meshes)
        {
            mesh.material = mat;
        }
    }

    public void RotateWheels()
    {
        if (TryGetComponent<VehicleWheels>(out var wheels))
        {
            wheels.rotateWheelsAppearaing(3, 500);
        }
    }
}
