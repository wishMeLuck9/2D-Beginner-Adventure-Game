using UnityEngine;

public sealed class ParticleSelfDestruct : MonoBehaviour
{
    [SerializeField] private float lifeTime = 2f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
