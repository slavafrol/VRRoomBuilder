using System.Collections.Generic;
using UnityEngine;

namespace RoomBuilder.Catalog
{
    [CreateAssetMenu(menuName = "Room Builder/Furniture Catalog")]
    public class FurnitureCatalog : ScriptableObject
    {
        public List<FurnitureItemDefinition> items = new List<FurnitureItemDefinition>();

        public FurnitureItemDefinition GetById(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return null;

            foreach (FurnitureItemDefinition item in items)
            {
                if (item == null)
                    continue;

                if (item.itemId == itemId)
                    return item;
            }

            return null;
        }
    }
}