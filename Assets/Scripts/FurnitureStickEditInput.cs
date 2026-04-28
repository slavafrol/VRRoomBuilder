using UnityEngine;
using System.Collections;

namespace RoomBuilder.Runtime
{
    public class FurnitureStickEditInput : MonoBehaviour
    {
        private enum MoveDirectionMode
        {
            FromHeadToObject,
            PointerForward
        }

        [Header("References")]
        [SerializeField] private FurnitureSelectionManager selectionManager;
        [SerializeField] private Transform head;
        [SerializeField] private Transform pointerOrigin;

        [Header("Input")]
        [SerializeField] private OVRInput.Controller controller = OVRInput.Controller.RTouch;
        [SerializeField] private OVRInput.Axis2D thumbstickAxis = OVRInput.Axis2D.PrimaryThumbstick;

        [Header("Rotation")]
        [SerializeField] private float rotationStepDegrees = 11.25f;
        [SerializeField] private float rotationDeadzone = 0.75f;
        [SerializeField] private float rotationRepeatDelay = 0.1f;

        [Header("Forward / Back Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float moveDeadzone = 0.8f;
        private float smoothedMoveInput;
        [SerializeField] private LayerMask pointerMoveMask;
        [SerializeField] private float pointerMoveRayDistance = 10f;

        [Header("Edit Mode Lock")]
        [SerializeField] private GameObject[] gameObjectsToDisableWhileSelected;

        [Header("Deselect")]
        [SerializeField] private OVRInput.Button deselectButton = OVRInput.Button.Two;
        [SerializeField] private OVRInput.Controller deselectController = OVRInput.Controller.RTouch;

        private Coroutine rotationRoutine;

        private float nextAllowedRotationTime;

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
        }

        private void Update()
        {
            if (selectionManager == null || selectionManager.CurrentSelection == null)
                return;

            if (OVRInput.GetDown(deselectButton, deselectController))
            {
                selectionManager.ClearSelection();
                return;
            }

            Vector2 stick = OVRInput.Get(thumbstickAxis, controller);
            HandleRotation(stick.x);
            HandleForwardBackMovement(stick.y);
        }

        private void HandleRotation(float x)
        {
            if (Mathf.Abs(x) < rotationDeadzone)
                return;

            if (rotationRoutine != null)
                return;

            if (Time.time < nextAllowedRotationTime)
                return;

            float direction = x > 0f ? 1f : -1f;
            float degrees = direction * rotationStepDegrees;

            rotationRoutine = StartCoroutine(SmoothRotationStep(degrees));
            nextAllowedRotationTime = Time.time + rotationRepeatDelay;
        }

        private IEnumerator SmoothRotationStep(float degrees)
        {
            SpawnedFurniture selectedFurniture = selectionManager.CurrentSelection;

            if (selectedFurniture == null)
            {
                rotationRoutine = null;
                yield break;
            }

            Transform selectedTransform = selectedFurniture.transform;

            float startYaw = selectedTransform.eulerAngles.y;
            float targetYaw = startYaw + degrees;

            float duration = Mathf.Max(0.01f, rotationRepeatDelay);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (selectionManager == null || selectionManager.CurrentSelection != selectedFurniture)
                {
                    rotationRoutine = null;
                    yield break;
                }

                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                float yaw = Mathf.LerpAngle(startYaw, targetYaw, smoothT);
                selectedTransform.rotation = Quaternion.Euler(0f, yaw, 0f);

                yield return null;
            }

            if (selectionManager != null && selectionManager.CurrentSelection == selectedFurniture)
                selectedTransform.rotation = Quaternion.Euler(0f, targetYaw, 0f);

            rotationRoutine = null;
        }

        private void HandleForwardBackMovement(float y)
        {
            float targetInput = 0f;

            if (Mathf.Abs(y) >= moveDeadzone)
            {
                float inputStrength = Mathf.InverseLerp(moveDeadzone, 1f, Mathf.Abs(y));
                inputStrength = Mathf.SmoothStep(0f, 1f, inputStrength);
                targetInput = inputStrength * Mathf.Sign(y);
            }

            smoothedMoveInput = Mathf.MoveTowards(
                smoothedMoveInput,
                targetInput,
                6f * Time.deltaTime
            );

            if (Mathf.Abs(smoothedMoveInput) < 0.001f)
            {
                smoothedMoveInput = 0f;
                return;
            }

            Vector3 direction = GetMoveDirection(y);

            float distance = smoothedMoveInput * moveSpeed * Time.deltaTime;

            selectionManager.MoveSelectedAlongGround(direction, distance);
        }

        private Vector3 GetMoveDirection(float y)
        {
            Transform selected = selectionManager.CurrentSelection.transform;

            if (pointerOrigin != null)
            {
                return GetPointerRayMoveDirection(selected, y);
            }

            if (head != null)
            {
                Vector3 fromHeadToObject = selected.position - head.position;
                fromHeadToObject.y = 0f;

                if (fromHeadToObject.sqrMagnitude > 0.001f)
                    return fromHeadToObject.normalized;

                Vector3 headForward = head.forward;
                headForward.y = 0f;
                return headForward.normalized;
            }

            Vector3 fallbackForward = selected.forward;
            fallbackForward.y = 0f;
            return fallbackForward.normalized;
        }

        private void HandleSelectionChanged(SpawnedFurniture selectedFurniture)
        {
            bool hasSelection = selectedFurniture != null;

            foreach (GameObject obj in gameObjectsToDisableWhileSelected)
            {
                if (obj != null)
                    obj.SetActive(!hasSelection);
            }
        }

        private Vector3 GetPointerRayMoveDirection(Transform selected, float y)
        {
            if (y < 0f)
            {
                Vector3 directionFromOriginToObject = selected.position - pointerOrigin.position;
                directionFromOriginToObject.y = 0f;

                if (directionFromOriginToObject.sqrMagnitude < 0.001f)
                {
                    directionFromOriginToObject = -pointerOrigin.forward;
                    directionFromOriginToObject.y = 0f;
                }

                return directionFromOriginToObject.normalized;
            }

            Ray ray = new Ray(pointerOrigin.position, pointerOrigin.forward);

            Vector3 targetPoint;

            if (Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    pointerMoveRayDistance,
                    pointerMoveMask,
                    QueryTriggerInteraction.Ignore))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = pointerOrigin.position + pointerOrigin.forward * pointerMoveRayDistance;
            }

            Vector3 directionToRayTarget = targetPoint - selected.position;
            directionToRayTarget.y = 0f;

            if (directionToRayTarget.sqrMagnitude < 0.001f)
            {
                directionToRayTarget = pointerOrigin.forward;
                directionToRayTarget.y = 0f;
            }

            return directionToRayTarget.normalized;
        }
    }
}