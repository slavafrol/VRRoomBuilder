using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoomBuilder.Save
{
    [Serializable]
    public class RoomSaveData
    {
        public int version = 1;
        public string roomName = "My Room";
        public List<FurnitureObjectSaveData> furniture = new List<FurnitureObjectSaveData>();
        public string wallMaterialName;
    }

    [Serializable]
    public class FurnitureObjectSaveData
    {
        public string itemId;

        public Vector3 position;
        public Vector3 rotationEuler;
        public Vector3 scale;

        // optional future fields
        public string colorId;
        public string materialId;
    }
}