using UnityEngine;
using MedievalResourceCollection.Gameplay;

namespace MedievalResourceCollection.Presentation
{
    [RequireComponent(typeof(Base))]
    [RequireComponent(typeof(ResourceStorage))]
    public class BaseStatusView : MonoBehaviour
    {
        [SerializeField] private Vector3 _offset = new Vector3(0f, 4f, 0f);
        [SerializeField] private Vector2 _size = new Vector2(160f, 44f);

        private Base _base;
        private Camera _camera;
        private GUIStyle _style;
        private ResourceStorage _storage;
        private string _status;

        private void Awake()
        {
            _base = GetComponent<Base>();
            _camera = Camera.main;
            _storage = GetComponent<ResourceStorage>();
        }

        private void OnEnable()
        {
            _base.UnitAdded += HandleUnitChanged;
            _base.UnitRemoved += HandleUnitChanged;
            _storage.AmountChanged += HandleAmountChanged;
            ShowStatus();
        }

        private void Start()
        {
            ShowStatus();
        }

        private void OnDisable()
        {
            _base.UnitAdded -= HandleUnitChanged;
            _base.UnitRemoved -= HandleUnitChanged;
            _storage.AmountChanged -= HandleAmountChanged;
        }

        private void OnGUI()
        {
            if (_camera == null)
                return;

            Vector3 screenPosition = _camera.WorldToScreenPoint(transform.position + _offset);

            if (screenPosition.z <= 0f)
                return;

            CreateStyle();

            Rect area = new Rect(
                screenPosition.x - (_size.x / 2f),
                Screen.height - screenPosition.y - (_size.y / 2f),
                _size.x,
                _size.y);

            GUI.Box(area, string.Empty);
            GUI.Label(area, _status, _style);
        }

        private void HandleAmountChanged(int amount)
        {
            ShowStatus();
        }

        private void HandleUnitChanged(Base resourceBase, Unit unit)
        {
            ShowStatus();
        }

        private void CreateStyle()
        {
            if (_style != null)
                return;

            _style = new GUIStyle(GUI.skin.label);
            _style.alignment = TextAnchor.MiddleCenter;
            _style.fontSize = 16;
            _style.normal.textColor = Color.white;
        }

        private void ShowStatus()
        {
            _status = $"Юниты: {_base.Units.Count}\nРесурсы: {_storage.Amount}";
        }
    }
}
