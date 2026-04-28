using RoomBuilder.Catalog;
using RoomBuilder.Save;
using UnityEngine;

namespace RoomBuilder.Runtime
{
    public class SpawnedFurniture : MonoBehaviour
    {
        public FurnitureItemDefinition Definition { get; private set; }

        [SerializeField] private FurnitureSelectionHighlight selectionHighlight;

        private FurnitureScaleLimiter scaleLimiter;

        public void Initialize(FurnitureItemDefinition definition)
        {
            Definition = definition;

            scaleLimiter = GetComponent<FurnitureScaleLimiter>();

            if (scaleLimiter == null)
                scaleLimiter = gameObject.AddComponent<FurnitureScaleLimiter>();

            scaleLimiter.Configure(definition.minUniformScale, definition.maxUniformScale);

            if (selectionHighlight == null)
                selectionHighlight = GetComponent<FurnitureSelectionHighlight>();
        }

        public FurnitureObjectSaveData ToSaveData()
        {
            return new FurnitureObjectSaveData
            {
                itemId = Definition != null ? Definition.itemId : string.Empty,
                position = transform.position,
                rotationEuler = transform.eulerAngles,
                scale = transform.localScale
            };
        }

        public void ApplySaveData(FurnitureObjectSaveData data)
        {
            transform.position = data.position;
            transform.rotation = Quaternion.Euler(data.rotationEuler);
            transform.localScale = data.scale;
        }

        public void SetSelected(bool selected)
        {
            if (selectionHighlight != null)
                selectionHighlight.SetHighlighted(selected);
        }

        public void Delete()
        {
            Destroy(gameObject);
        }
    }
}