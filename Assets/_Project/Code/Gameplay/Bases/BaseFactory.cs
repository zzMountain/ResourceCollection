using UnityEngine;

namespace MedievalResourceCollection.Gameplay
{
    public class BaseFactory : MonoBehaviour
    {
        [SerializeField] private Base _basePrefab;
        [SerializeField] private Unit _unitPrefab;

        public Base CreateBase(Vector3 position)
        {
            Base resourceBase = Instantiate(_basePrefab, position, Quaternion.identity);
            resourceBase.SetUnderConstruction();
            return resourceBase;
        }

        public Unit CreateUnit(Base resourceBase)
        {
            Unit unit = Instantiate(_unitPrefab, resourceBase.UnitSpawnPosition, Quaternion.identity);
            return unit;
        }
    }
}
