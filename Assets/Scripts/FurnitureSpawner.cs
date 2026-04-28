using RoomBuilder.Catalog;
using RoomBuilder.Save;
using UnityEngine;

namespace RoomBuilder.Runtime
{
    public class FurnitureSpawner : MonoBehaviour
    {
        [Header("Placement")]
        [SerializeField] private FurniturePlacementPoseProvider placementPoseProvider;
        [SerializeField] private Transform spawnedFurnitureParent;

        [Header("Selection")]
        [SerializeField] private FurnitureSelectionManager selectionManager;

        public Transform SpawnedFurnitureParent => spawnedFurnitureParent;

        public SpawnedFurniture Spawn(FurnitureItemDefinition item)
        {
            if (item == null)
            {
                Debug.LogWarning("Cannot spawn furniture: item is null.");
                return null;
            }

            if (item.prefab == null)
            {
                Debug.LogWarning($"Cannot spawn furniture: prefab is missing for {item.displayName}.");
                return null;
            }

            if (placementPoseProvider == null)
            {
                Debug.LogError("FurnitureSpawner has no FurniturePlacementPoseProvider assigned.");
                return null;
            }

            if (!placementPoseProvider.TryGetPlacementPose(out Pose pose))
            {
                Debug.LogWarning("No valid placement pose found.");
                return null;
            }

            SpawnedFurniture spawnedFurniture = CreateInstance(
                item,
                pose.position,
                pose.rotation * Quaternion.Euler(item.spawnRotationEuler),
                item.spawnScale
            );

            if (selectionManager != null && spawnedFurniture != null)
                selectionManager.Select(spawnedFurniture);

            return spawnedFurniture;
        }

        public SpawnedFurniture SpawnFromSave(
            FurnitureItemDefinition item,
            FurnitureObjectSaveData saveData
        )
        {
            if (item == null || saveData == null)
                return null;

            SpawnedFurniture spawnedFurniture = CreateInstance(
                item,
                saveData.position,
                Quaternion.Euler(saveData.rotationEuler),
                saveData.scale
            );

            if (spawnedFurniture != null)
                spawnedFurniture.ApplySaveData(saveData);

            return spawnedFurniture;
        }

        private SpawnedFurniture CreateInstance(
            FurnitureItemDefinition item,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale
        )
        {
            GameObject instance = Instantiate(
                item.prefab,
                position,
                rotation,
                spawnedFurnitureParent
            );

            instance.transform.localScale = scale;

            SpawnedFurniture spawnedFurniture = instance.GetComponent<SpawnedFurniture>();

            if (spawnedFurniture == null)
                spawnedFurniture = instance.AddComponent<SpawnedFurniture>();

            spawnedFurniture.Initialize(item);

            FurnitureSelectable selectable = instance.GetComponent<FurnitureSelectable>();

            if (selectable == null)
                selectable = instance.AddComponent<FurnitureSelectable>();

            selectable.Initialize(spawnedFurniture, selectionManager);

            EnsureBasicPhysics(instance);

            return spawnedFurniture;
        }

        private static void EnsureBasicPhysics(GameObject instance)
        {
            Collider collider = instance.GetComponentInChildren<Collider>();

            if (collider == null)
                instance.AddComponent<BoxCollider>();

            Rigidbody rigidbody = instance.GetComponent<Rigidbody>();

            if (rigidbody == null)
            {
                rigidbody = instance.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
            }
        }
    }
}