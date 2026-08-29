using System;
using UnityEngine;

namespace MedievalResourceCollection.Gameplay
{
    public class ResourceStorage : MonoBehaviour
    {
        public event Action<int> AmountChanged;

        public int Amount { get; private set; }

        public void Add(int value)
        {
            Amount += value;
            AmountChanged?.Invoke(Amount);
        }

        public bool TrySpend(int value)
        {
            if (Amount < value)
                return false;

            Amount -= value;
            AmountChanged?.Invoke(Amount);
            return true;
        }
    }
}
