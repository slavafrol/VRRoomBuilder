using UnityEngine;

namespace RoomBuilder.Catalog
{
    public enum FurniturePlacementSurface
    {
        Floor,
        Wall,
        Any
    }

    public enum FurnitureCategory
    {
        Seating,
        Tables,
        Storage,
        Lights,
        Beds
    }

    [CreateAssetMenu(menuName = "Room Builder/Furniture Item")]
    public class FurnitureItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string itemId;
        public FurnitureCategory category;

        [Header("Display")]
        public string displayName;
        public Sprite thumbnail;

        [Header("Prefab")]
        public GameObject prefab;

        [Header("Spawn Settings")]
        public FurniturePlacementSurface placementSurface = FurniturePlacementSurface.Floor;
        public Vector3 spawnScale = Vector3.one;
        public Vector3 spawnRotationEuler = Vector3.zero;

        [Header("Scaling")]
        public float minUniformScale = 0.5f;
        public float maxUniformScale = 2.0f;

    }
}