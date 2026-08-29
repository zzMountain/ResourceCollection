using System;
using UnityEngine;

namespace MedievalResourceCollection.Gameplay
{
    public class Unit : MonoBehaviour
    {
        [SerializeField] private UnitMover _mover;
        [SerializeField] private Transform _carryPoint;

        private Base _baseBeingBuilt;
        private Base _owner;
        private Resource _resource;
        private bool _isDelivering;
        private bool _isBuilding;

        public event Action<Unit, Resource> AssignmentCancelled;
        public event Action<Unit, Base> BaseConstructionCompleted;
        public event Action<Unit, Resource> ResourceDelivered;

        public bool CanAcceptResource => _resource == null && _isBuilding == false;

        public Base Owner => _owner;

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

        public void AssignToBase(Base resourceBase)
        {
            _owner = resourceBase;
        }

        public void AssignResource(Resource resource)
        {
            if (CanAcceptResource == false)
                return;

            _resource = resource;
            _isDelivering = false;
            _mover.MoveTo(resource.transform.position);
        }

        public void BeginBaseConstruction(Base baseBeingBuilt)
        {
            _baseBeingBuilt = baseBeingBuilt;
            _isBuilding = true;
            _mover.MoveTo(baseBeingBuilt.transform.position);
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
            if (_isBuilding)
            {
                CompleteBaseConstruction();
                return;
            }

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
            _mover.MoveTo(_owner.DeliveryPoint.position);
        }

        private void DeliverResource()
        {
            Resource deliveredResource = _resource;

            _resource = null;
            _isDelivering = false;
            ResourceDelivered?.Invoke(this, deliveredResource);
        }

        private void CompleteBaseConstruction()
        {
            Base completedBase = _baseBeingBuilt;

            _baseBeingBuilt = null;
            _isBuilding = false;
            BaseConstructionCompleted?.Invoke(this, completedBase);
        }
    }
}
