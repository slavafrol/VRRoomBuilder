using UnityEngine;

namespace RoomBuilder.Runtime
{
    public class FurnitureSelectable : MonoBehaviour
    {
        private SpawnedFurniture spawnedFurniture;
        private FurnitureSelectionManager selectionManager;

        public void Initialize(
            SpawnedFurniture furniture,
            FurnitureSelectionManager manager
        )
        {
            spawnedFurniture = furniture;
            selectionManager = manager;
        }

        public void SelectSelf()
        {
            if (spawnedFurniture == null)
                spawnedFurniture = GetComponent<SpawnedFurniture>();

            if (selectionManager == null)
            {
                Debug.LogWarning($"{name}: Selection manager is missing.");
                return;
            }

            if (spawnedFurniture == null)
            {
                Debug.LogWarning($"{name}: SpawnedFurniture is missing.");
                return;
            }

            selectionManager.Select(spawnedFurniture);
            Debug.Log($"Selected furniture with ray/grab: {name}");
        }
    }
}