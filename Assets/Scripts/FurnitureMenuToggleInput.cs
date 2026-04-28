using UnityEngine;

namespace RoomBuilder.UI
{
    public class FurnitureMenuToggleInput : MonoBehaviour
    {
        [Header("Menu")]
        [SerializeField] private GameObject furnitureMenuRoot;

        [Header("Input")]
        [SerializeField] private OVRInput.Button toggleButton = OVRInput.Button.One;
        [SerializeField] private OVRInput.Controller controller = OVRInput.Controller.RTouch;

        private void Update()
        {
            if (OVRInput.GetDown(toggleButton, controller))
                ToggleMenu();
        }

        private void ToggleMenu()
        {
            if (furnitureMenuRoot == null)
            {
                Debug.LogError("FurnitureMenuToggleInput has no FurnitureMenuRoot assigned");
                return;
            }
            bool shouldShow = !furnitureMenuRoot.activeSelf;

            furnitureMenuRoot.SetActive(shouldShow);
        }
    }
}