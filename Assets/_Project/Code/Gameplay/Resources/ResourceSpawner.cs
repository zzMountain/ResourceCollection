using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MedievalResourceCollection.Gameplay
{
    public class ResourceSpawner : MonoBehaviour
    {
        [SerializeField] private Resource _resourcePrefab;
        [SerializeField] private Transform _spawnCenter;
        [SerializeField] private Transform _baseTransform;
        [SerializeField] private float _spawnInterval = 2f;
        [SerializeField] private int _maximumResourceCount = 10;
        [SerializeField] private float _halfWidth = 14f;
        [SerializeField] private float _halfDepth = 9f;
        [SerializeField] private float _baseExclusionRadius = 3.5f;
        [SerializeField] private float _minimumResourceDistance = 1.5f;
        [SerializeField] private int _maximumPositionAttempts = 20;

        private readonly HashSet<Resource> _resources = new HashSet<Resource>();
        private Coroutine _spawnCoroutine;
        private WaitForSeconds _spawnWait;

        private void Awake()
        {
            _spawnWait = new WaitForSeconds(_spawnInterval);
        }

        private void OnEnable()
        {
            _spawnCoroutine = StartCoroutine(SpawnResources());
        }

        private void OnDisable()
        {
            StopSpawning();
        }

        private void OnDestroy()
        {
            foreach (Resource resource in _resources)
                resource.Collected -= HandleResourceCollected;
        }

        private IEnumerator SpawnResources()
        {
            while (enabled)
            {
                yield return _spawnWait;

                if (_resources.Count < _maximumResourceCount && TryGetSpawnPosition(out Vector3 position))
                    CreateResource(position);
            }

            _spawnCoroutine = null;
        }

        private void CreateResource(Vector3 position)
        {
            Resource resource = Instantiate(_resourcePrefab, position, Quaternion.Euler(0f, 45f, 0f));
            resource.Collected += HandleResourceCollected;
            _resources.Add(resource);
        }

        private bool TryGetSpawnPosition(out Vector3 position)
        {
            for (int attempt = 0; attempt < _maximumPositionAttempts; attempt++)
            {
                float x = Random.Range(-_halfWidth, _halfWidth);
                float z = Random.Range(-_halfDepth, _halfDepth);
                Vector3 candidate = _spawnCenter.position + new Vector3(x, 0.75f, z);

                if (IsPositionAvailable(candidate))
                {
                    position = candidate;
                    return true;
                }
            }

            position = default;
            return false;
        }

        private bool IsPositionAvailable(Vector3 candidate)
        {
            if (Vector3.Distance(candidate, _baseTransform.position) < _baseExclusionRadius)
                return false;

            foreach (Resource resource in _resources)
            {
                if (Vector3.Distance(candidate, resource.transform.position) < _minimumResourceDistance)
                    return false;
            }

            return true;
        }

        private void HandleResourceCollected(Resource resource)
        {
            resource.Collected -= HandleResourceCollected;
            _resources.Remove(resource);
            Destroy(resource.gameObject);
        }

        private void StopSpawning()
        {
            if (_spawnCoroutine == null)
                return;

            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
    }
}
