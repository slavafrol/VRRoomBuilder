using System;
using UnityEngine;

namespace RoomBuilder.Runtime
{
    public class FurnitureSelectionManager : MonoBehaviour
    {
        public SpawnedFurniture CurrentSelection { get; private set; }

        public event Action<SpawnedFurniture> SelectionChanged;

        public void Select(SpawnedFurniture furniture)
        {
            if (CurrentSelection == furniture)
                return;

            if (CurrentSelection != null)
                CurrentSelection.SetSelected(false);

            CurrentSelection = furniture;

            if (CurrentSelection != null)
                CurrentSelection.SetSelected(true);

            SelectionChanged?.Invoke(CurrentSelection);
        }

        public void ClearSelection()
        {
            if (CurrentSelection != null)
                CurrentSelection.SetSelected(false);

            CurrentSelection = null;
            SelectionChanged?.Invoke(null);
        }

        public void DeleteSelected()
        {
            if (CurrentSelection == null)
                return;

            SpawnedFurniture toDelete = CurrentSelection;
            CurrentSelection = null;

            SelectionChanged?.Invoke(null);
            toDelete.Delete();
        }

        public void RotateSelectedYaw(float degrees)
        {
            if (CurrentSelection == null)
                return;

            Transform target = CurrentSelection.transform;

            float currentYaw = target.eulerAngles.y;
            float nextYaw = currentYaw + degrees;

            target.rotation = Quaternion.Euler(0f, nextYaw, 0f);
        }

        public void MoveSelectedAlongGround(Vector3 direction, float distance)
        {
            if (CurrentSelection == null)
                return;

            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
                return;

            Transform target = CurrentSelection.transform;
            target.position += direction.normalized * distance;
        }
    }
}