using UnityEngine;

namespace MedievalResourceCollection.Gameplay
{
    public class MapBounds : MonoBehaviour
    {
        [SerializeField] private Vector2 _halfExtents = new Vector2(14f, 9f);

        public bool Contains(Vector3 position)
        {
            Vector3 offset = position - transform.position;
            return Mathf.Abs(offset.x) <= _halfExtents.x && Mathf.Abs(offset.z) <= _halfExtents.y;
        }
    }
}
