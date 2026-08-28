using System;
using UnityEngine;

namespace MedievalResourceCollection.Gameplay
{
    public class Unit : MonoBehaviour
    {
        [SerializeField] private UnitMover _mover;
        [SerializeField] private Transform _carryPoint;

        private Transform _deliveryPoint;
        private Resource _resource;
        private bool _isDelivering;

        public event Action<int> ResourceDelivered;

        public bool IsAvailable => _resource == null;

        private void OnEnable()
        {
            _mover.DestinationReached += HandleDestinationReached;
        }

        private void OnDisable()
        {
            _mover.DestinationReached -= HandleDestinationReached;
            _mover.Stop();
            CancelAssignment();
        }

        public void Initialize(Transform deliveryPoint)
        {
            _deliveryPoint = deliveryPoint;
        }

        public bool TryAssign(Resource resource)
        {
            if (IsAvailable == false || resource.TryReserve(this) == false)
                return false;

            _resource = resource;
            _isDelivering = false;
            _mover.MoveTo(resource.transform.position);
            return true;
        }

        public void CancelAssignment()
        {
            if (_resource == null)
                return;

            _resource.TryRelease(this);
            _resource = null;
            _isDelivering = false;
        }

        private void HandleDestinationReached()
        {
            if (_resource == null)
                return;

            if (_isDelivering)
            {
                DeliverResource();
                return;
            }

            PickUpResource();
        }

        private void PickUpResource()
        {
            if (_resource.TryPickUp(this, _carryPoint) == false)
            {
                CancelAssignment();
                return;
            }

            _isDelivering = true;
            _mover.MoveTo(_deliveryPoint.position);
        }

        private void DeliverResource()
        {
            Resource deliveredResource = _resource;

            if (deliveredResource.TryCollect(this, out int value))
                ResourceDelivered?.Invoke(value);

            _resource = null;
            _isDelivering = false;
        }
    }
}
