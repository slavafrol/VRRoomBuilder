using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoomBuilder.UI
{
    public class WallMaterialCardUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text materialNameText;
        [SerializeField] private Image thumbnail;
        [SerializeField] private Button button;

        private Material material;
        private WallMaterialBrowserUI browser;

        public void Bind(Material materialToBind, WallMaterialBrowserUI owner)
        {
            material = materialToBind;
            browser = owner;

            if (materialNameText != null)
                materialNameText.text = material != null ? material.name : "Missing";

            SetupPreview();

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClicked);
            }
        }

        private void OnClicked()
        {
            browser.ApplyMaterial(material);
        }

        private void SetupPreview()
        {
            if (thumbnail == null || material == null)
                return;

            thumbnail.sprite = null;

            Texture mainTexture = null;

            if (material.HasProperty("_BaseMap"))
                mainTexture = material.GetTexture("_BaseMap");
            else if (material.HasProperty("_MainTex"))
                mainTexture = material.GetTexture("_MainTex");

            if (mainTexture is Texture2D texture2D)
            {
                thumbnail.sprite = Sprite.Create(
                    texture2D,
                    new Rect(0, 0, texture2D.width, texture2D.height),
                    new Vector2(0.5f, 0.5f)
                );

                thumbnail.color = Color.white;
                return;
            }

            if (material.HasProperty("_BaseColor"))
                thumbnail.color = material.GetColor("_BaseColor");
            else if (material.HasProperty("_Color"))
                thumbnail.color = material.GetColor("_Color");
            else
                thumbnail.color = Color.gray;
        }
    }
}