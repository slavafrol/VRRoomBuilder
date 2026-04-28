using RoomBuilder.Catalog;
using RoomBuilder.Runtime;
using UnityEngine;

namespace RoomBuilder.UI
{
    public class FurnitureMenuController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private FurnitureCatalog catalog;

        [Header("UI")]
        [SerializeField] private Transform cardContainer;
        [SerializeField] private FurnitureCardUI cardPrefab;

        [Header("Runtime")]
        [SerializeField] private FurnitureSpawner furnitureSpawner;

        [Header("Selection UI")]
        [SerializeField] private FurnitureSelectionManager selectionManager;
        [SerializeField] private GameObject deleteButtonRoot;

        private bool useCategoryFilter;
        private FurnitureCategory activeCategory;

        private void OnEnable()
        {
            if (selectionManager != null)
                selectionManager.SelectionChanged += HandleSelectionChanged;
        }

        private void OnDisable()
        {
            if (selectionManager != null)
                selectionManager.SelectionChanged -= HandleSelectionChanged;
        }

        private void Start()
        {
            HandleSelectionChanged(selectionManager.CurrentSelection);
            BuildMenu();
        }

        public void BuildMenu()
        {
            if (catalog == null)
            {
                Debug.LogError("FurnitureMenuController has no catalog assigned.");
                return;
            }

            if (cardContainer == null || cardPrefab == null)
            {
                Debug.LogError("FurnitureMenuController UI references are missing.");
                return;
            }

            ClearMenu();

            foreach (FurnitureItemDefinition item in catalog.items)
            {
                if (item == null)
                    continue;

                if (useCategoryFilter && item.category != activeCategory)
                    continue;

                FurnitureCardUI card = Instantiate(cardPrefab, cardContainer);
                card.Setup(item, HandlePlaceClicked);
            }
        }

        private void ClearMenu()
        {
            for (int i = cardContainer.childCount - 1; i >= 0; i--)
                Destroy(cardContainer.GetChild(i).gameObject);
        }

        private void HandlePlaceClicked(FurnitureItemDefinition item)
        {
            if (furnitureSpawner == null)
            {
                Debug.LogError("FurnitureMenuController has no FurnitureSpawner assigned.");
                return;
            }

            furnitureSpawner.Spawn(item);
        }

        private void HandleSelectionChanged(SpawnedFurniture selectedFurniture)
        {
            bool hasSelection = selectedFurniture != null;

            if (deleteButtonRoot != null)
                deleteButtonRoot.SetActive(hasSelection);
        }

        public void ShowAll()
        {
            useCategoryFilter = false;
            BuildMenu();
        }

        public void ShowSeating()
        {
            ShowCategory(FurnitureCategory.Seating);
        }

        public void ShowTables()
        {
            ShowCategory(FurnitureCategory.Tables);
        }

        public void ShowStorage()
        {
            ShowCategory(FurnitureCategory.Storage);
        }

        public void ShowBeds()
        {
            ShowCategory(FurnitureCategory.Beds);
        }

        private void ShowCategory(FurnitureCategory category)
        {
            useCategoryFilter = true;
            activeCategory = category;
            BuildMenu();
        }

    }
}