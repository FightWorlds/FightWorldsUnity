using UnityEngine;

public class GridObject
{
    public Transform VisualTransform;
    public bool IsFilled;
    public int X { get; private set; }
    public int Z { get; private set; }
    private Material fillMaterial;

    public GridObject(Material material, int x, int z)
    {
        this.fillMaterial = material;
        this.X = x;
        this.Z = z;
    }

    public void FillHex()
    {
        if (IsFilled)
            return;
        IsFilled = true;
        VisualTransform.GetChild(0).GetComponent<MeshRenderer>()
        .material = fillMaterial;
    }
}
