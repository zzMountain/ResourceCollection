using System.Collections.Generic;
using UnityEngine;

namespace MedievalResourceCollection.Gameplay
{
    public class Base : MonoBehaviour
    {
        [SerializeField] private ResourceStorage _storage;
        [SerializeField] private ResourceScanner _scanner;
        [SerializeField] private Transform _deliveryPoint;
        [SerializeField] private Unit[] _units;

        private void Awake()
        {
            foreach (Unit unit in _units)
                unit.Initialize(_deliveryPoint);
        }

        private void OnEnable()
        {
            _scanner.ResourcesFound += HandleResourcesFound;

            foreach (Unit unit in _units)
                unit.ResourceDelivered += HandleResourceDelivered;
        }

        private void OnDisable()
        {
            _scanner.ResourcesFound -= HandleResourcesFound;

            foreach (Unit unit in _units)
                unit.ResourceDelivered -= HandleResourceDelivered;
        }

        private void HandleResourcesFound(IReadOnlyCollection<Resource> resources)
        {
            foreach (Unit unit in _units)
            {
                if (unit.IsAvailable == false)
                    continue;

                foreach (Resource resource in resources)
                {
                    if (unit.TryAssign(resource))
                        break;
                }
            }
        }

        private void HandleResourceDelivered(int value)
        {
            _storage.Add(value);
        }
    }
}
