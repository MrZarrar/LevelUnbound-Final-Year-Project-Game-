using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AutoDestroyVFX : MonoBehaviour
{
    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        // When the particle system stops playing, destroy it
        if (ps != null && !ps.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}