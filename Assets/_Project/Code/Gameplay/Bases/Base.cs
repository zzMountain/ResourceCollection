using System;
using System.Collections.Generic;
using UnityEngine;

namespace MedievalResourceCollection.Gameplay
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(ResourceStorage))]
    public class Base : MonoBehaviour
    {
        private const int BaseConstructionCost = 5;
        private const int UnitProductionCost = 3;

        [SerializeField] private Unit[] _initialUnits;
        [SerializeField] private Vector3 _unitSpawnOffset = new Vector3(0f, 0f, -2f);

        private readonly List<Unit> _units = new List<Unit>();
        private Collider _collider;
        private BaseFlag _flag;
        private Renderer[] _renderers;
        private ResourceStorage _storage;
        private bool _isConstructionInProgress;
        private bool _isConstructed = true;
        private bool _isUsingResources;

        public event Action<Base, Unit> UnitAdded;
        public event Action<Base, Unit> UnitRemoved;
        public event Action<Base> UnitProductionRequested;
        public event Action<Base, Unit, Vector3> BaseConstructionRequested;

        public bool CanSelect => _isConstructed;

        public Transform DeliveryPoint => transform;

        public IReadOnlyCollection<Unit> Units => _units;

        public Vector3 UnitSpawnPosition => transform.TransformPoint(_unitSpawnOffset);

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _renderers = GetComponentsInChildren<Renderer>();
            _storage = GetComponent<ResourceStorage>();

            foreach (Unit unit in _initialUnits)
                _units.Add(unit);
        }

        private void OnEnable()
        {
            _storage.AmountChanged += HandleAmountChanged;
        }

        private void OnDisable()
        {
            _storage.AmountChanged -= HandleAmountChanged;
        }

        public void AddUnit(Unit unit)
        {
            if (_units.Contains(unit))
                return;

            _units.Add(unit);
            UnitAdded?.Invoke(this, unit);
        }

        public void AddResource(int value)
        {
            _storage.Add(value);
        }

        public void CompleteConstruction()
        {
            _isConstructed = true;
            SetStructureVisible(true);
            TryUseResources();
        }

        public void FinishConstruction()
        {
            _isConstructionInProgress = false;
            RemoveFlag();
            TryUseResources();
        }

        public void RemoveUnit(Unit unit)
        {
            if (_units.Remove(unit) == false)
                return;

            UnitRemoved?.Invoke(this, unit);
        }

        public void SetFlag(BaseFlag flag)
        {
            RemoveFlag();
            _flag = flag;
            TryUseResources();
        }

        public void SetUnderConstruction()
        {
            _isConstructed = false;
            SetStructureVisible(false);
        }

        private void HandleAmountChanged(int amount)
        {
            TryUseResources();
        }

        private void TryUseResources()
        {
            if (_isConstructed == false || _isConstructionInProgress || _isUsingResources)
                return;

            _isUsingResources = true;

            if (_flag != null && _units.Count > 1)
            {
                TryBuildBase();
            }
            else
            {
                TryProduceUnits();
            }

            _isUsingResources = false;
        }

        private void TryProduceUnits()
        {
            while (_storage.TrySpend(UnitProductionCost))
                UnitProductionRequested?.Invoke(this);
        }

        private void TryBuildBase()
        {
            if (_units.Count <= 1)
                return;

            if (TryGetFreeUnit(out Unit builder) == false)
                return;

            _isConstructionInProgress = true;

            if (_storage.TrySpend(BaseConstructionCost) == false)
            {
                _isConstructionInProgress = false;
                return;
            }

            BaseConstructionRequested?.Invoke(this, builder, _flag.transform.position);
        }

        private bool TryGetFreeUnit(out Unit unit)
        {
            foreach (Unit candidate in _units)
            {
                if (candidate.CanAcceptResource)
                {
                    unit = candidate;
                    return true;
                }
            }

            unit = null;
            return false;
        }

        private void RemoveFlag()
        {
            if (_flag == null)
                return;

            Destroy(_flag.gameObject);
            _flag = null;
        }

        private void SetStructureVisible(bool isVisible)
        {
            _collider.enabled = isVisible;

            foreach (Renderer renderer in _renderers)
                renderer.enabled = isVisible;
        }
    }
}
