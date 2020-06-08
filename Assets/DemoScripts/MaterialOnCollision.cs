using UnityEngine;


public class MaterialOnCollision : MonoBehaviour
{
    public MeshRenderer rendererToChange;
    public Material[]   materials;

    public Light   lightToChange;
    public Color[] lightColours;

    void Start()
    {
        materialIndex = 0;
        lightIndex    = 0;
        ApplyMaterials();
    }

    public void ChangeMaterial()
    {
        materialIndex = (materialIndex + 1) % materials.Length;
        lightIndex    = (lightIndex    + 1) % lightColours.Length;
        ApplyMaterials();
    }

    private void ApplyMaterials()
    {
        rendererToChange.material = materials[materialIndex];
        lightToChange.color       = lightColours[lightIndex];
    }


    private void OnTriggerEnter(Collider other)
    {
        ChangeMaterial();
    }

    private void OnTriggerExit(Collider other)
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private int materialIndex;
    private int lightIndex;
}
