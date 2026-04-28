using UnityEngine;

namespace RoomBuilder.Runtime
{
    public class ScaleHandleController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FurnitureSelectionManager selectionManager;
        [SerializeField] private Transform head;
        [SerializeField] private GameObject handleRoot;
        [SerializeField] private Transform handleTransform;

        [Header("Handle Position")]
        [SerializeField] private float distance = 0.3f;
        [SerializeField] private float heightOffset = -0.2f;
        [SerializeField] private float sideOffset = 0.1f;

        [Header("Pull Limits")]
        [SerializeField] private Vector3 maxPullOffset = new Vector3(0.2f, 0.2f, 0.2f);

        [Header("Scale Mapping")]
        [SerializeField] private Vector3 scalePerMeter = new Vector3(2f, 2f, 2f);

        [Header("Axis Visuals")]
        [SerializeField] private GameObject axisVisualRoot;

        [SerializeField] private LineRenderer xLine;
        [SerializeField] private LineRenderer yLine;
        [SerializeField] private LineRenderer zLine;

        [SerializeField] private Transform xArrowHead;
        [SerializeField] private Transform yArrowHead;
        [SerializeField] private Transform zArrowHead;

        [SerializeField] private float minVisibleAxisLength = 0.01f;

        [SerializeField] private Transform buttonPanel;
        [SerializeField] private Vector3 buttonsOffsetFromHandle = new Vector3(0f, -0.18f, 0f);

        [SerializeField] private bool fixedProportions = false;

        private SpawnedFurniture selectedFurniture;
        private Transform selectedTransform;
        private FurnitureScaleLimiter selectedScaleLimiter;

        private bool isGrabbed;

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
            axisVisualRoot.SetActive(false);
            HandleSelectionChanged(selectionManager != null ? selectionManager.CurrentSelection : null);
        }

        private void LateUpdate()
        {
            if (selectedFurniture == null || head == null || handleTransform == null)
            {
                axisVisualRoot.SetActive(false);
                return;
            }

            GetAnchorFrame(
                out Vector3 anchorPosition,
                out Vector3 right,
                out Vector3 up,
                out Vector3 forward
            );

            UpdateButtonsPanel(anchorPosition, right, up, forward);

            if (!isGrabbed)
            {
                handleTransform.position = anchorPosition;
                axisVisualRoot.SetActive(false);
                return;
            }

            Vector3 worldOffset = handleTransform.position - anchorPosition;

            Vector3 localOffset = new Vector3(
                Vector3.Dot(worldOffset, right),
                Vector3.Dot(worldOffset, up),
                Vector3.Dot(worldOffset, forward)
            );

            localOffset.x = Mathf.Clamp(localOffset.x, -maxPullOffset.x, maxPullOffset.x);
            localOffset.y = Mathf.Clamp(localOffset.y, -maxPullOffset.y, maxPullOffset.y);
            localOffset.z = Mathf.Clamp(localOffset.z, -maxPullOffset.z, maxPullOffset.z);

            handleTransform.position =
                anchorPosition
                + right * localOffset.x
                + up * localOffset.y
                + forward * localOffset.z;

            UpdateAxisVisuals(anchorPosition, right, up, forward, localOffset);

            ApplyScaleFromOffset(localOffset);
        }

        public void BeginGrab()
        {
            if (selectedTransform == null)
                return;

            isGrabbed = true;
        }

        public void EndGrab()
        {
            isGrabbed = false;
            SnapHandleBack();
        }

        private void HandleSelectionChanged(SpawnedFurniture furniture)
        {
            selectedFurniture = furniture;
            selectedTransform = furniture != null ? furniture.transform : null;

            selectedScaleLimiter = selectedTransform != null
                ? selectedTransform.GetComponent<FurnitureScaleLimiter>()
                : null;

            isGrabbed = false;

            bool hasSelection = selectedFurniture != null;

            if (handleRoot != null)
                handleRoot.SetActive(hasSelection);

            if (buttonPanel != null)
                buttonPanel.gameObject.SetActive(hasSelection);

            if (hasSelection)
                SnapHandleBack();
        }

        private void ApplyScaleFromOffset(Vector3 localOffset)
        {
            if (selectedTransform == null)
                return;

            if (localOffset.sqrMagnitude < 0.0001f)
                return;

            Vector3 scaleChangeThisFrame = Vector3.Scale(localOffset, scalePerMeter) * Time.deltaTime;

            if (fixedProportions)
            {
                float uniformChange = GetDominantSignedValue(scaleChangeThisFrame);
                scaleChangeThisFrame = Vector3.one * uniformChange;
            }

            Vector3 desiredScale = selectedTransform.localScale + scaleChangeThisFrame;

            if (selectedScaleLimiter != null)
                selectedScaleLimiter.SetScaleClamped(desiredScale);
            else
                selectedTransform.localScale = desiredScale;
        }

        private void SnapHandleBack()
        {
            if (head == null || handleTransform == null)
                return;

            GetAnchorFrame(
                out Vector3 anchorPosition,
                out _,
                out _,
                out _
            );

            handleTransform.position = anchorPosition;
        }

        private void GetAnchorFrame(
            out Vector3 anchorPosition,
            out Vector3 right,
            out Vector3 up,
            out Vector3 forward
        )
        {
            forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;

            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;

            right = Vector3.Cross(Vector3.up, forward).normalized;
            up = Vector3.up;

            anchorPosition =
                head.position
                + forward * distance
                + right * sideOffset
                + up * heightOffset;
        }

        private void UpdateAxisVisuals(
            Vector3 anchorPosition,
            Vector3 right,
            Vector3 up,
            Vector3 forward,
            Vector3 localOffset
        )
        {
            if (axisVisualRoot != null)
                axisVisualRoot.SetActive(true);

            UpdateSingleAxis(
                xLine,
                xArrowHead,
                anchorPosition,
                right,
                localOffset.x
            );

            UpdateSingleAxis(
                yLine,
                yArrowHead,
                anchorPosition,
                up,
                localOffset.y
            );

            UpdateSingleAxis(
                zLine,
                zArrowHead,
                anchorPosition,
                forward,
                localOffset.z
            );
        }

        private void UpdateSingleAxis(
            LineRenderer line,
            Transform arrowHead,
            Vector3 anchorPosition,
            Vector3 axisDirection,
            float axisDistance
        )
        {
            bool shouldShow = Mathf.Abs(axisDistance) >= minVisibleAxisLength;

            if (line != null)
                line.enabled = shouldShow;

            if (arrowHead != null)
                arrowHead.gameObject.SetActive(shouldShow);

            if (!shouldShow)
                return;

            Vector3 normalizedAxis = axisDirection.normalized;

            Vector3 endPoint =
                anchorPosition
                + normalizedAxis * axisDistance;

            Vector3 arrowDirection =
                axisDistance >= 0f
                    ? normalizedAxis
                    : -normalizedAxis;

            if (line != null)
            {
                line.positionCount = 2;
                line.SetPosition(0, anchorPosition);
                line.SetPosition(1, endPoint);
            }

            if (arrowHead != null)
            {
                arrowHead.position = endPoint;
                arrowDirection.Normalize();
                arrowHead.rotation = Quaternion.FromToRotation(Vector3.up, arrowDirection);
            }
        }

        private void UpdateButtonsPanel(
            Vector3 anchorPosition,
            Vector3 right,
            Vector3 up,
            Vector3 forward
        )
        {
            if (buttonPanel == null)
                return;

            buttonPanel.position =
                anchorPosition
                + right * buttonsOffsetFromHandle.x
                + up * buttonsOffsetFromHandle.y
                + forward * buttonsOffsetFromHandle.z;

            Vector3 toHead = head.position - buttonPanel.position;
            toHead.y = 0f;

            if (toHead.sqrMagnitude < 0.001f)
                toHead = -forward;

            buttonPanel.rotation = Quaternion.LookRotation(toHead.normalized, Vector3.up) * Quaternion.Euler(-25f, 180f, 0f);
        }

        private float GetDominantSignedValue(Vector3 value)
        {
            float absX = Mathf.Abs(value.x);
            float absY = Mathf.Abs(value.y);
            float absZ = Mathf.Abs(value.z);

            if (absX >= absY && absX >= absZ)
                return value.x;

            if (absY >= absX && absY >= absZ)
                return value.y;

            return value.z;
        }

        public void ResetSelectedScale()
        {
            if (selectedTransform == null)
                return;

            Vector3 resetScale = Vector3.one;

            if (selectedFurniture != null && selectedFurniture.Definition != null)
                resetScale = selectedFurniture.Definition.spawnScale;

            selectedTransform.localScale = resetScale;
        }

        public void ToggleFixedProportions()
        {
            fixedProportions = !fixedProportions;
        }
    }
}