using UnityEngine;
using UnityEngine.UI;
using MedievalResourceCollection.Gameplay;

namespace MedievalResourceCollection.UI
{
    public class ResourceAmountView : MonoBehaviour
    {
        [SerializeField] private ResourceStorage _storage;
        [SerializeField] private Text _text;

        private void OnEnable()
        {
            _storage.AmountChanged += HandleAmountChanged;
            ShowAmount(_storage.Amount);
        }

        private void OnDisable()
        {
            _storage.AmountChanged -= HandleAmountChanged;
        }

        private void HandleAmountChanged(int amount)
        {
            ShowAmount(amount);
        }

        private void ShowAmount(int amount)
        {
            _text.text = $"Ресурсы: {amount}";
        }
    }
}
