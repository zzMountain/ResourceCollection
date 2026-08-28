using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MedievalResourceCollection.Gameplay
{
    public class ResourceScanner : MonoBehaviour
    {
        [SerializeField] private Transform _scanCenter;
        [SerializeField] private Vector3 _halfExtents = new Vector3(15f, 2f, 10f);
        [SerializeField] private LayerMask _resourceLayerMask = ~0;
        [SerializeField] private float _scanInterval = 0.75f;

        private readonly HashSet<Resource> _foundResources = new HashSet<Resource>();
        private Coroutine _scanCoroutine;
        private WaitForSeconds _scanWait;

        public event Action<IReadOnlyCollection<Resource>> ResourcesFound;

        private void Awake()
        {
            _scanWait = new WaitForSeconds(_scanInterval);
        }

        private void OnEnable()
        {
            _scanCoroutine = StartCoroutine(ScanResources());
        }

        private void OnDisable()
        {
            StopScanning();
        }

        private IEnumerator ScanResources()
        {
            while (enabled)
            {
                yield return _scanWait;
                _foundResources.Clear();
                Collider[] colliders = Physics.OverlapBox(
                    _scanCenter.position,
                    _halfExtents,
                    Quaternion.identity,
                    _resourceLayerMask);

                foreach (Collider collider in colliders)
                {
                    if (collider.TryGetComponent(out Resource resource) && resource.IsAvailable)
                        _foundResources.Add(resource);
                }

                if (_foundResources.Count > 0)
                    ResourcesFound?.Invoke(_foundResources);
            }

            _scanCoroutine = null;
        }

        private void StopScanning()
        {
            if (_scanCoroutine == null)
                return;

            StopCoroutine(_scanCoroutine);
            _scanCoroutine = null;
        }
    }
}
