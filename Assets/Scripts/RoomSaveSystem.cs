using RoomBuilder.Catalog;
using RoomBuilder.Runtime;
using RoomBuilder.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

namespace RoomBuilder.Save
{
    public class RoomSaveSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FurnitureCatalog catalog;
        [SerializeField] private FurnitureSpawner spawner;
        [SerializeField] private FurnitureSelectionManager selectionManager;

        [Header("Save Settings")]
        [SerializeField] private string saveFileName = "room_save";
        [SerializeField] private string savesFolderName = "Rooms";
        [SerializeField] private TMP_InputField saveNameInput;
        [SerializeField] private GameObject saveField;
        [SerializeField] private GameObject roomBrowserPanel;
        [SerializeField] private GameObject wallMaterialBrowserPanel;
        [SerializeField] private WallMaterialBrowserUI wallMaterialBrowserUI;

        [Header("Thumbnail")]
        [SerializeField] private Camera thumbnailCamera;
        [SerializeField] private int thumbnailWidth = 512;
        [SerializeField] private int thumbnailHeight = 512;

        private string SaveDirectory => Path.Combine(Application.persistentDataPath, savesFolderName);

        private void Awake()
        {
            Directory.CreateDirectory(SaveDirectory);
        }

        public void SaveRoom()
        {
            string inputName = saveNameInput != null ? saveNameInput.text : string.Empty;

            if (string.IsNullOrWhiteSpace(inputName))
            {
                Debug.LogWarning("Save name is empty.");
                return;
            }

            saveFileName = MakeSafeFileName(inputName) + ".json";
            SaveRoomAs(saveFileName);
        }

        public void SaveRoomAs(string requestedSaveName)
        {
            string safeSaveName = SanitizeSaveName(requestedSaveName);

            if (string.IsNullOrWhiteSpace(safeSaveName))
            {
                Debug.LogWarning("Save cancelled: invalid or empty save name.");
                return;
            }

            if (spawner == null || spawner.SpawnedFurnitureParent == null)
            {
                Debug.LogError("Cannot save room: spawner or spawned furniture parent is missing.");
                return;
            }

            RoomSaveData roomSaveData = new RoomSaveData();

            roomSaveData.wallMaterialName = wallMaterialBrowserUI.CurrentMaterialName;

            SpawnedFurniture[] spawnedFurniture =
                spawner.SpawnedFurnitureParent.GetComponentsInChildren<SpawnedFurniture>();

            foreach (SpawnedFurniture furniture in spawnedFurniture)
            {
                if (furniture == null || furniture.Definition == null)
                    continue;

                FurnitureObjectSaveData objectSaveData = furniture.ToSaveData();

                if (string.IsNullOrWhiteSpace(objectSaveData.itemId))
                    continue;

                roomSaveData.furniture.Add(objectSaveData);
            }

            string json = JsonUtility.ToJson(roomSaveData, true);
            File.WriteAllText(GetJsonPath(safeSaveName), json);

            CaptureThumbnail(safeSaveName);

            Debug.Log($"Room saved: {safeSaveName}");
            ToggleSave();
        }

        public void LoadRoom()
        {
            LoadRoom(saveFileName);
        }

        public void LoadRoom(string requestedSaveName)
        {
            string safeSaveName = SanitizeSaveName(requestedSaveName);
            string jsonPath = GetJsonPath(safeSaveName);

            if (!File.Exists(jsonPath))
            {
                Debug.LogWarning($"No room save found at: {jsonPath}");
                return;
            }

            string json = File.ReadAllText(jsonPath);
            RoomSaveData roomSaveData = JsonUtility.FromJson<RoomSaveData>(json);

            if (roomSaveData == null)
            {
                Debug.LogError("Failed to read room save data.");
                return;
            }

            ClearCurrentRoom();

            foreach (FurnitureObjectSaveData objectSaveData in roomSaveData.furniture)
            {
                FurnitureItemDefinition item = catalog.GetById(objectSaveData.itemId);

                if (item == null)
                {
                    Debug.LogWarning($"Missing furniture item in catalog: {objectSaveData.itemId}");
                    continue;
                }

                spawner.SpawnFromSave(item, objectSaveData);
            }
            
            wallMaterialBrowserUI.ApplyMaterialByName(roomSaveData.wallMaterialName);

            Debug.Log($"Room loaded: {safeSaveName}");
            ToggleLoad();
        }

        public void ClearCurrentRoom()
        {
            if (selectionManager != null)
                selectionManager.ClearSelection();

            if (spawner == null || spawner.SpawnedFurnitureParent == null)
                return;

            for (int i = spawner.SpawnedFurnitureParent.childCount - 1; i >= 0; i--)
            {
                Destroy(spawner.SpawnedFurnitureParent.GetChild(i).gameObject);
            }

            wallMaterialBrowserUI.ApplyMaterialByName("");
        }

        public void DeleteSaveFile()
        {
            DeleteSaveFile(saveFileName);
        }

        public void DeleteSaveFile(string requestedSaveName)
        {
            string safeSaveName = SanitizeSaveName(requestedSaveName);

            string jsonPath = GetJsonPath(safeSaveName);
            string thumbnailPath = GetThumbnailPath(safeSaveName);

            if (File.Exists(jsonPath))
                File.Delete(jsonPath);

            if (File.Exists(thumbnailPath))
                File.Delete(thumbnailPath);

            Debug.Log($"Room save deleted: {safeSaveName}");
        }

        public List<RoomSaveEntry> GetAllRooms()
        {
            Directory.CreateDirectory(SaveDirectory);

            string[] jsonFiles = Directory.GetFiles(SaveDirectory, "*.json");
            List<RoomSaveEntry> rooms = new List<RoomSaveEntry>();

            foreach (string jsonPath in jsonFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(jsonPath);

                rooms.Add(new RoomSaveEntry
                {
                    fileName = fileName,
                    displayName = fileName,
                    jsonPath = jsonPath,
                    thumbnailPath = GetThumbnailPath(fileName),
                    lastWriteTime = File.GetLastWriteTime(jsonPath)
                });
            }

            rooms.Sort((a, b) => b.lastWriteTime.CompareTo(a.lastWriteTime));
            return rooms;
        }

        private void CaptureThumbnail(string safeSaveName)
        {
            if (thumbnailCamera == null)
            {
                Debug.LogWarning("Thumbnail was not captured because thumbnailCamera is not assigned.");
                return;
            }

            string thumbnailPath = GetThumbnailPath(safeSaveName);

            RenderTexture renderTexture = new RenderTexture(thumbnailWidth, thumbnailHeight, 24);
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = thumbnailCamera.targetTexture;

            try
            {
                thumbnailCamera.targetTexture = renderTexture;
                thumbnailCamera.Render();

                RenderTexture.active = renderTexture;

                Texture2D texture = new Texture2D(thumbnailWidth, thumbnailHeight, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, thumbnailWidth, thumbnailHeight), 0, 0);
                texture.Apply();

                byte[] pngBytes = texture.EncodeToPNG();
                File.WriteAllBytes(thumbnailPath, pngBytes);

                Destroy(texture);
            }
            finally
            {
                thumbnailCamera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                Destroy(renderTexture);
            }
        }

        private string GetJsonPath(string safeSaveName)
        {
            return Path.Combine(SaveDirectory, safeSaveName + ".json");
        }

        private string GetThumbnailPath(string safeSaveName)
        {
            return Path.Combine(SaveDirectory, safeSaveName + ".png");
        }

        private string SanitizeSaveName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
                return string.Empty;

            string safeName = rawName.Trim();

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
                safeName = safeName.Replace(invalidChar.ToString(), "");

            return safeName;
        }

        private string MakeSafeFileName(string rawName)
        {
            string safeName = rawName.Trim();

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
                safeName = safeName.Replace(invalidChar.ToString(), "");

            return safeName;
        }

        public void ToggleSave()
        {
            saveField.SetActive(!saveField.activeSelf);
            if (saveField.activeSelf)
            {

                int roomCount = Directory.GetFiles(SaveDirectory, "*.json").Length + 1;
                saveNameInput.text = "Room_" + roomCount.ToString();

                StartCoroutine(FocusSaveNameInputNextFrame());
            }
        }

        public void ToggleLoad()
        {
            roomBrowserPanel.SetActive(!roomBrowserPanel.activeSelf);
        }

        public void ToggleMaterialBrowser()
        {
            wallMaterialBrowserPanel.SetActive(!wallMaterialBrowserPanel.activeSelf);
        }

        private IEnumerator FocusSaveNameInputNextFrame()
        {
            yield return null;

            saveNameInput.Select();
            saveNameInput.ActivateInputField();
        }
    }

    [Serializable]
    public class RoomSaveEntry
    {
        public string fileName;
        public string displayName;
        public string jsonPath;
        public string thumbnailPath;
        public DateTime lastWriteTime;
    }
}