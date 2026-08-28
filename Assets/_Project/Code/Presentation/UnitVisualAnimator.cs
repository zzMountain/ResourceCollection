using UnityEngine;
using MedievalResourceCollection.Gameplay;

namespace MedievalResourceCollection.Presentation
{
    [RequireComponent(typeof(UnitMover))]
    public class UnitVisualAnimator : MonoBehaviour
    {
        [SerializeField] private Transform _visual;
        [SerializeField] private float _idleCycleSpeed = 1.4f;
        [SerializeField] private float _idleVerticalAmplitude = 0.025f;
        [SerializeField] private float _idleSwayAngle = 2.5f;
        [SerializeField] private float _walkCycleSpeed = 7f;
        [SerializeField] private float _walkVerticalAmplitude = 0.085f;
        [SerializeField] private float _walkSwayAngle = 8f;

        private UnitMover _mover;
        private Vector3 _initialLocalPosition;
        private Quaternion _initialLocalRotation;
        private float _cycleTime;

        private void Awake()
        {
            _mover = GetComponent<UnitMover>();
            _initialLocalPosition = _visual.localPosition;
            _initialLocalRotation = _visual.localRotation;
            _cycleTime = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            float cycleSpeed = _mover.IsMoving ? _walkCycleSpeed : _idleCycleSpeed;
            float verticalAmplitude = _mover.IsMoving ? _walkVerticalAmplitude : _idleVerticalAmplitude;
            float swayAngle = _mover.IsMoving ? _walkSwayAngle : _idleSwayAngle;

            _cycleTime += Time.deltaTime * cycleSpeed;

            float verticalOffset = Mathf.Abs(Mathf.Sin(_cycleTime)) * verticalAmplitude;
            float tiltAngle = Mathf.Sin(_cycleTime * 0.5f) * swayAngle;

            _visual.localPosition = _initialLocalPosition + (Vector3.up * verticalOffset);
            _visual.localRotation = _initialLocalRotation * Quaternion.Euler(0f, 0f, tiltAngle);
        }
    }
}
