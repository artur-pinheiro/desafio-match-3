using UnityEngine;
using UnityEngine.Pool;

public class ParticlePool : MonoBehaviour {
    [Header("Particle Settings")]
    [SerializeField] private ParticleSystem _particlePrefab;
    [SerializeField] private int _defaultPoolSize = 50; 

    private IObjectPool<ParticleSystem> _particlePool;

    void Awake() {
        _particlePool = new ObjectPool<ParticleSystem>(
            createFunc: CreateParticle,
            actionOnGet: OnParticleGet,
            actionOnRelease: OnParticleRelease,
            actionOnDestroy: OnParticleDestroy,
            defaultCapacity: _defaultPoolSize,
            maxSize: 100
        );
    }

    private ParticleSystem CreateParticle() {
        ParticleSystem particle = Instantiate(_particlePrefab);
        particle.gameObject.SetActive(false);

        var main = particle.main;
        main.stopAction = ParticleSystemStopAction.Callback;
        particle.gameObject.AddComponent<ParticleSystemCallbackReceiver>().Initialize(_particlePool, particle);

        return particle;
    }

    private void OnParticleGet(ParticleSystem particle) {
        particle.gameObject.SetActive(true);
        particle.Play();
    }

    private void OnParticleRelease(ParticleSystem particle) {
        particle.Stop();
        particle.gameObject.SetActive(false);
    }

    private void OnParticleDestroy(ParticleSystem particle) {
        Destroy(particle.gameObject);
    }

    public ParticleSystem SpawnParticle(Vector3 position, Quaternion rotation) {
        ParticleSystem particle = _particlePool.Get();
        particle.transform.position = position;
        particle.transform.rotation = rotation;
        return particle;
    }

    public void ReturnParticle(ParticleSystem particle) {
        _particlePool.Release(particle);
    }
}

public class ParticleSystemCallbackReceiver : MonoBehaviour {
    private IObjectPool<ParticleSystem> _pool;
    private ParticleSystem _particleSystem;

    public void Initialize(IObjectPool<ParticleSystem> pool, ParticleSystem particleSystem) {
        _pool = pool;
        _particleSystem = particleSystem;
    }

    // This method is called by Unity when the particle system stops playing
    private void OnParticleSystemStopped() {
        if ( _pool != null && _particleSystem != null ) {
            _pool.Release(_particleSystem);
        }
    }
}
