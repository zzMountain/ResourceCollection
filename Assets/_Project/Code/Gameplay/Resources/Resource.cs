using System;
using UnityEngine;

namespace MedievalResourceCollection.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class Resource : MonoBehaviour
    {
        [SerializeField] private int _value = 1;

        private Collider _collider;
        private Unit _owner;
        private ResourceState _state = ResourceState.Available;

        public event Action<Resource> Collected;

        private enum ResourceState
        {
            Available,
            Reserved,
            Carried,
            Collected
        }

        public bool IsAvailable => _state == ResourceState.Available;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        public bool TryReserve(Unit unit)
        {
            if (IsAvailable == false)
                return false;

            _owner = unit;
            _state = ResourceState.Reserved;
            return true;
        }

        public bool TryPickUp(Unit unit, Transform carryPoint)
        {
            if (_state != ResourceState.Reserved || _owner != unit)
                return false;

            transform.SetParent(carryPoint);
            transform.localPosition = Vector3.zero;
            _collider.enabled = false;
            _state = ResourceState.Carried;
            return true;
        }

        public bool TryCollect(Unit unit, out int value)
        {
            value = 0;

            if (_state != ResourceState.Carried || _owner != unit)
                return false;

            _state = ResourceState.Collected;
            _owner = null;
            value = _value;
            Collected?.Invoke(this);
            return true;
        }

        public bool TryRelease(Unit unit)
        {
            if (_state != ResourceState.Reserved && _state != ResourceState.Carried)
                return false;

            if (_owner != unit)
                return false;

            if (_state == ResourceState.Carried)
            {
                transform.SetParent(null);
                _collider.enabled = true;
            }

            _owner = null;
            _state = ResourceState.Available;
            return true;
        }
    }
}
