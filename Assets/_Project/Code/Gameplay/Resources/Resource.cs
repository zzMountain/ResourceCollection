using UnityEngine;

namespace MedievalResourceCollection.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class Resource : MonoBehaviour
    {
        [SerializeField] private int _value = 1;

        private Collider _collider;

        public int Value => _value;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        public void AttachTo(Transform carryPoint)
        {
            transform.SetParent(carryPoint);
            transform.localPosition = Vector3.zero;
            _collider.enabled = false;
        }

        public void Detach()
        {
            transform.SetParent(null, true);
            _collider.enabled = true;
        }
    }
}
