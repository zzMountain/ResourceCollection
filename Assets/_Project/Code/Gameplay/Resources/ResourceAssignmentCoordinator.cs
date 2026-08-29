using System.Collections.Generic;
using UnityEngine;

namespace MedievalResourceCollection.Gameplay
{
    public class ResourceAssignmentCoordinator : MonoBehaviour
    {
        [SerializeField] private ResourceStorage _storage;
        [SerializeField] private ResourceScanner _scanner;
        [SerializeField] private ResourceSpawner _spawner;
        [SerializeField] private Transform _deliveryPoint;
        [SerializeField] private Unit[] _units;

        private readonly Dictionary<Unit, Resource> _assignments = new Dictionary<Unit, Resource>();
        private readonly HashSet<Resource> _assignedResources = new HashSet<Resource>();
        private readonly HashSet<Resource> _freeResources = new HashSet<Resource>();

        private void Awake()
        {
            foreach (Unit unit in _units)
                unit.Initialize(_deliveryPoint);
        }

        private void OnEnable()
        {
            _scanner.ResourcesFound += HandleResourcesFound;

            foreach (Unit unit in _units)
            {
                unit.AssignmentCancelled += HandleAssignmentCancelled;
                unit.ResourceDelivered += HandleResourceDelivered;
            }
        }

        private void OnDisable()
        {
            _scanner.ResourcesFound -= HandleResourcesFound;

            foreach (Unit unit in _units)
            {
                unit.AssignmentCancelled -= HandleAssignmentCancelled;
                unit.ResourceDelivered -= HandleResourceDelivered;
            }

            foreach (Unit unit in _units)
                unit.CancelAssignment();

            _assignments.Clear();
            _assignedResources.Clear();
            _freeResources.Clear();
        }

        private void HandleResourcesFound(IReadOnlyCollection<Resource> resources)
        {
            foreach (Resource resource in resources)
            {
                if (_assignedResources.Contains(resource) == false)
                    _freeResources.Add(resource);
            }

            TryAssignResources();
        }

        private void HandleAssignmentCancelled(Unit unit, Resource resource)
        {
            if (TryRemoveAssignment(unit, resource) == false)
                return;

            _freeResources.Add(resource);
            TryAssignResources();
        }

        private void HandleResourceDelivered(Unit unit, Resource resource)
        {
            if (TryRemoveAssignment(unit, resource) == false)
                return;

            _storage.Add(resource.Value);
            _spawner.Remove(resource);
            TryAssignResources();
        }

        private void TryAssignResources()
        {
            foreach (Unit unit in _units)
            {
                if (unit.isActiveAndEnabled == false || unit.CanAcceptResource == false)
                    continue;

                if (TryGetFreeResource(out Resource resource) == false)
                    return;

                AssignResource(unit, resource);
            }
        }

        private void AssignResource(Unit unit, Resource resource)
        {
            if (_freeResources.Remove(resource) == false)
                return;

            _assignedResources.Add(resource);
            _assignments.Add(unit, resource);
            unit.AssignResource(resource);
        }

        private bool TryGetFreeResource(out Resource resource)
        {
            foreach (Resource candidate in _freeResources)
            {
                resource = candidate;
                return true;
            }

            resource = null;
            return false;
        }

        private bool TryRemoveAssignment(Unit unit, Resource resource)
        {
            if (_assignments.TryGetValue(unit, out Resource assignedResource) == false)
                return false;

            if (assignedResource != resource)
                return false;

            _assignments.Remove(unit);
            _assignedResources.Remove(resource);
            return true;
        }
    }
}
