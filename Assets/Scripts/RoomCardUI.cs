using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoomBuilder.Save
{
    public class RoomCardUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text roomNameText;
        [SerializeField] private TMP_Text dateText;
        [SerializeField] private Image thumbnailImage;
        [SerializeField] private Sprite fallbackThumbnail;

        private string fileName;
        private RoomBrowserUI browser;

        public void Bind(RoomSaveEntry entry, RoomBrowserUI owner)
        {
            fileName = entry.fileName;
            browser = owner;

            if (roomNameText != null)
                roomNameText.text = entry.displayName;

            if (dateText != null)
                dateText.text = entry.lastWriteTime.ToString("g");

            if (thumbnailImage != null)
                thumbnailImage.sprite = LoadThumbnail(entry.thumbnailPath);
        }

        public void OnLoadClicked()
        {
            if (browser != null)
                browser.LoadRoom(fileName);
        }

        public void OnDeleteClicked()
        {
            if (browser != null)
                browser.DeleteRoom(fileName);
        }

        private Sprite LoadThumbnail(string path)
        {
            if (!File.Exists(path))
                return fallbackThumbnail;

            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);

            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }
    }
}