using UnityEngine;
using UnityEngine.InputSystem;

namespace MedievalResourceCollection.Gameplay
{
    public class BaseSelectionInput : MonoBehaviour
    {
        [SerializeField] private BaseFlag _flagPrefab;
        [SerializeField] private Camera _camera;
        [SerializeField] private MapBounds _mapBounds;

        private Base _selectedBase;

        private void Update()
        {
            Mouse mouse = Mouse.current;

            if (mouse == null || mouse.leftButton.wasPressedThisFrame == false)
                return;

            Ray ray = _camera.ScreenPointToRay(mouse.position.ReadValue());

            if (_selectedBase == null)
            {
                SelectBase(ray);
                return;
            }

            PlaceFlag(ray);
        }

        private void SelectBase(Ray ray)
        {
            if (Physics.Raycast(ray, out RaycastHit hit) == false)
                return;

            if (hit.collider.TryGetComponent(out Base resourceBase) == false)
                return;

            if (resourceBase.CanSelect == false)
                return;

            _selectedBase = resourceBase;
        }

        private void PlaceFlag(Ray ray)
        {
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float distance) == false)
                return;

            Vector3 position = ray.GetPoint(distance);

            if (_mapBounds.Contains(position) == false)
                return;

            BaseFlag flag = Instantiate(_flagPrefab, position, Quaternion.identity);
            _selectedBase.SetFlag(flag);
            _selectedBase = null;
        }
    }
}
