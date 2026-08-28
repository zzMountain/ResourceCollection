using System;
using UnityEngine;

namespace MedievalResourceCollection.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class UnitMover : MonoBehaviour
    {
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _rotationSpeed = 540f;
        [SerializeField] private float _stoppingDistance = 0.12f;

        private Rigidbody _rigidbody;
        private Vector3 _target;
        private bool _isMoving;

        public event Action DestinationReached;

        public bool IsMoving => _isMoving;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (_isMoving == false)
                return;

            Vector3 currentPosition = _rigidbody.position;
            Vector3 targetPosition = new Vector3(_target.x, currentPosition.y, _target.z);
            Vector3 remainingOffset = targetPosition - currentPosition;
            float remainingDistance = remainingOffset.magnitude;

            if (remainingDistance <= _stoppingDistance)
            {
                _rigidbody.MovePosition(targetPosition);
                _isMoving = false;
                DestinationReached?.Invoke();
                return;
            }

            Vector3 direction = remainingOffset / remainingDistance;
            float stepDistance = _speed * Time.fixedDeltaTime;
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            Quaternion rotation = Quaternion.RotateTowards(
                _rigidbody.rotation,
                targetRotation,
                _rotationSpeed * Time.fixedDeltaTime);
            Vector3 nextPosition = currentPosition + (direction * Mathf.Min(stepDistance, remainingDistance));

            _rigidbody.MoveRotation(rotation);
            _rigidbody.MovePosition(nextPosition);
        }

        public void MoveTo(Vector3 target)
        {
            _target = target;
            _isMoving = true;
        }

        public void Stop()
        {
            _isMoving = false;
        }
    }
}
