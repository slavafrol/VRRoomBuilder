using RoomBuilder.Catalog;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoomBuilder.UI
{
    public class FurnitureCardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image thumbnailImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button button;

        private FurnitureItemDefinition item;
        private System.Action<FurnitureItemDefinition> onClicked;

        public void Setup(
            FurnitureItemDefinition itemDefinition,
            System.Action<FurnitureItemDefinition> clickCallback
        )
        {
            item = itemDefinition;
            onClicked = clickCallback;

            if (titleText != null)
                titleText.text = item != null ? item.displayName : "Missing";

            if (thumbnailImage != null)
            {
                thumbnailImage.sprite = item != null ? item.thumbnail : null;
                thumbnailImage.enabled = thumbnailImage.sprite != null;
            }

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(HandleClicked);
            }
        }

        private void HandleClicked()
        {
            if (item == null)
                return;

            onClicked?.Invoke(item);
        }
    }
}