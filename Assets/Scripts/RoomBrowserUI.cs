using System.IO;
using TMPro;
using UnityEngine;

namespace RoomBuilder.Save
{
    public class RoomBrowserUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RoomSaveSystem roomSaveSystem;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private RoomCardUI roomCardPrefab;

        private void OnEnable()
        {
            RefreshBrowser();
        }

        public void RefreshBrowser()
        {
            if (roomSaveSystem == null || contentRoot == null || roomCardPrefab == null)
                return;

            for (int i = contentRoot.childCount - 1; i >= 0; i--)
                Destroy(contentRoot.GetChild(i).gameObject);

            var rooms = roomSaveSystem.GetAllRooms();

            foreach (var room in rooms)
            {
                RoomCardUI card = Instantiate(roomCardPrefab, contentRoot);
                card.Bind(room, this);
            }
        }

        public void LoadRoom(string fileName)
        {
            if (roomSaveSystem == null)
                return;

            roomSaveSystem.LoadRoom(fileName);
        }

        public void DeleteRoom(string fileName)
        {
            if (roomSaveSystem == null)
                return;

            roomSaveSystem.DeleteSaveFile(fileName);
            RefreshBrowser();
        }
    }
}