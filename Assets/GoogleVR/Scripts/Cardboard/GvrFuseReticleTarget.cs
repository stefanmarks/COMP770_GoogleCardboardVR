using UnityEngine;

public class GvrFuseReticleTarget : MonoBehaviour
{
    [Tooltip("Time that the reticle fuse needs to run to activate this object")]
    [Range(0.01f, 10.0f)]
    public float FuseTime = 1.0f;

    public void Start()
    {
        // only here to activate enable/disable flag
    }
}
