using System.Collections.Generic;
using UnityEngine;

namespace MedievalResourceCollection.Gameplay
{
    public class ResourceAssignmentCoordinator : MonoBehaviour
    {
        [SerializeField] private ResourceScanner _scanner;
        [SerializeField] private ResourceSpawner _spawner;
        [SerializeField] private BaseFactory _factory;
        [SerializeField] private Base[] _bases;

        private readonly Dictionary<Base, Base> _constructionSources = new Dictionary<Base, Base>();
        private readonly Dictionary<Unit, Resource> _assignments = new Dictionary<Unit, Resource>();
        private readonly HashSet<Base> _registeredBases = new HashSet<Base>();
        private readonly HashSet<Resource> _assignedResources = new HashSet<Resource>();
        private readonly HashSet<Resource> _freeResources = new HashSet<Resource>();
        private readonly List<Base> _knownBases = new List<Base>();
        private readonly HashSet<Unit> _registeredUnits = new HashSet<Unit>();

        private void Awake()
        {
            foreach (Base resourceBase in _bases)
                _knownBases.Add(resourceBase);
        }

        private void OnEnable()
        {
            _scanner.ResourcesFound += HandleResourcesFound;

            foreach (Base resourceBase in _knownBases)
                RegisterBase(resourceBase);
        }

        private void OnDisable()
        {
            _scanner.ResourcesFound -= HandleResourcesFound;

            Unit[] registeredUnits = new Unit[_registeredUnits.Count];
            _registeredUnits.CopyTo(registeredUnits);

            foreach (Unit unit in registeredUnits)
                UnregisterUnit(unit);

            foreach (Unit unit in registeredUnits)
                unit.CancelAssignment();

            Base[] registeredBases = new Base[_registeredBases.Count];
            _registeredBases.CopyTo(registeredBases);

            foreach (Base resourceBase in registeredBases)
                UnregisterBase(resourceBase);

            _constructionSources.Clear();
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

        private void HandleUnitAdded(Base resourceBase, Unit unit)
        {
            RegisterUnit(unit, resourceBase);
            TryAssignResources();
        }

        private void HandleUnitRemoved(Base resourceBase, Unit unit)
        {
            UnregisterUnit(unit);
        }

        private void HandleUnitProductionRequested(Base resourceBase)
        {
            Unit unit = _factory.CreateUnit(resourceBase);
            resourceBase.AddUnit(unit);
        }

        private void HandleBaseConstructionRequested(Base sourceBase, Unit builder, Vector3 position)
        {
            Base newBase = _factory.CreateBase(position);

            sourceBase.RemoveUnit(builder);
            RegisterBase(newBase);
            newBase.AddUnit(builder);
            _constructionSources.Add(newBase, sourceBase);
            builder.BeginBaseConstruction(newBase);
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

            unit.Owner.AddResource(resource.Value);
            _spawner.Remove(resource);
            TryAssignResources();
        }

        private void HandleBaseConstructionCompleted(Unit unit, Base newBase)
        {
            if (_constructionSources.TryGetValue(newBase, out Base sourceBase) == false)
                return;

            _constructionSources.Remove(newBase);
            newBase.CompleteConstruction();
            sourceBase.FinishConstruction();
            TryAssignResources();
        }

        private void RegisterBase(Base resourceBase)
        {
            if (_registeredBases.Add(resourceBase) == false)
                return;

            if (_knownBases.Contains(resourceBase) == false)
                _knownBases.Add(resourceBase);

            resourceBase.UnitAdded += HandleUnitAdded;
            resourceBase.UnitRemoved += HandleUnitRemoved;
            resourceBase.UnitProductionRequested += HandleUnitProductionRequested;
            resourceBase.BaseConstructionRequested += HandleBaseConstructionRequested;

            foreach (Unit unit in resourceBase.Units)
                RegisterUnit(unit, resourceBase);
        }

        private void UnregisterBase(Base resourceBase)
        {
            if (_registeredBases.Remove(resourceBase) == false)
                return;

            resourceBase.UnitAdded -= HandleUnitAdded;
            resourceBase.UnitRemoved -= HandleUnitRemoved;
            resourceBase.UnitProductionRequested -= HandleUnitProductionRequested;
            resourceBase.BaseConstructionRequested -= HandleBaseConstructionRequested;
        }

        private void RegisterUnit(Unit unit, Base resourceBase)
        {
            if (_registeredUnits.Add(unit) == false)
                return;

            unit.AssignToBase(resourceBase);
            unit.AssignmentCancelled += HandleAssignmentCancelled;
            unit.BaseConstructionCompleted += HandleBaseConstructionCompleted;
            unit.ResourceDelivered += HandleResourceDelivered;
        }

        private void UnregisterUnit(Unit unit)
        {
            if (_registeredUnits.Remove(unit) == false)
                return;

            if (_assignments.TryGetValue(unit, out Resource resource))
            {
                _assignments.Remove(unit);
                _assignedResources.Remove(resource);
                _freeResources.Add(resource);
            }

            unit.AssignmentCancelled -= HandleAssignmentCancelled;
            unit.BaseConstructionCompleted -= HandleBaseConstructionCompleted;
            unit.ResourceDelivered -= HandleResourceDelivered;
        }

        private void TryAssignResources()
        {
            foreach (Unit unit in _registeredUnits)
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
