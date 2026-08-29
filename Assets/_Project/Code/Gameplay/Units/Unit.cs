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

        public event Action<Unit, Resource> AssignmentCancelled;
        public event Action<Unit, Resource> ResourceDelivered;

        public bool CanAcceptResource => _resource == null;

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

        public void AssignResource(Resource resource)
        {
            if (CanAcceptResource == false)
                return;

            _resource = resource;
            _isDelivering = false;
            _mover.MoveTo(resource.transform.position);
        }

        public void CancelAssignment()
        {
            if (_resource == null)
                return;

            Resource cancelledResource = _resource;

            if (_isDelivering)
                cancelledResource.Detach();

            _resource = null;
            _isDelivering = false;
            AssignmentCancelled?.Invoke(this, cancelledResource);
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
            _resource.AttachTo(_carryPoint);
            _isDelivering = true;
            _mover.MoveTo(_deliveryPoint.position);
        }

        private void DeliverResource()
        {
            Resource deliveredResource = _resource;

            _resource = null;
            _isDelivering = false;
            ResourceDelivered?.Invoke(this, deliveredResource);
        }
    }
}
