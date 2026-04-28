using System.Collections.Generic;
using UnityEngine;

namespace RoomBuilder.UI
{
    public class WallMaterialBrowserUI : MonoBehaviour
    {
        [Header("Material Source")]
        [SerializeField] private string resourcesFolderPath = "WallMaterials";
        [SerializeField] private Material defaultMaterial;

        [Header("Walls")]
        [SerializeField] private Transform wallsRoot;
        [SerializeField] private Renderer[] wallRenderers;
        [SerializeField] private int materialSlotIndex = 0;

        [Header("UI")]
        [SerializeField] private Transform contentRoot;
        [SerializeField] private WallMaterialCardUI cardPrefab;

        private readonly List<Material> materials = new List<Material>();

        public string CurrentMaterialName { get; private set; }

        private void OnEnable()
        {
            RefreshBrowser();
        }

        public void RefreshBrowser()
        {
            LoadMaterials();
            RefreshWallRenderers();
            ClearCards();

            foreach (Material material in materials)
            {
                if (material == null)
                    continue;

                WallMaterialCardUI card = Instantiate(cardPrefab, contentRoot);
                card.Bind(material, this);
            }
        }

        public void ApplyMaterial(Material material)
        {
            if (material == null)
                return;

            RefreshWallRenderers();

            foreach (Renderer wallRenderer in wallRenderers)
            {
                if (wallRenderer == null)
                    continue;

                Material[] sharedMaterials = wallRenderer.sharedMaterials;

                if (sharedMaterials == null || sharedMaterials.Length == 0)
                    continue;

                if (materialSlotIndex < 0 || materialSlotIndex >= sharedMaterials.Length)
                    continue;

                sharedMaterials[materialSlotIndex] = material;
                wallRenderer.sharedMaterials = sharedMaterials;
            }

            CurrentMaterialName = material.name;
        }

        public void ApplyMaterialByName(string materialName)
        {
            if (string.IsNullOrWhiteSpace(materialName))
            {
                ApplyMaterial(defaultMaterial);
                return;
            }

            LoadMaterials();

            foreach (Material material in materials)
            {
                if (material == null)
                    continue;

                if (material.name == materialName)
                {
                    ApplyMaterial(material);
                    return;
                }
            }
        }

        private void LoadMaterials()
        {
            materials.Clear();

            Material[] loadedMaterials = Resources.LoadAll<Material>(resourcesFolderPath);

            foreach (Material material in loadedMaterials)
            {
                if (material != null)
                    materials.Add(material);
            }
        }

        private void RefreshWallRenderers()
        {
            if (wallRenderers != null && wallRenderers.Length > 0)
                return;

            if (wallsRoot == null)
                return;

            wallRenderers = wallsRoot.GetComponentsInChildren<Renderer>(true);
        }

        private void ClearCards()
        {
            if (contentRoot == null)
                return;

            for (int i = contentRoot.childCount - 1; i >= 0; i--)
                Destroy(contentRoot.GetChild(i).gameObject);
        }
    }
}