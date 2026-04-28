using UnityEngine;

namespace RoomBuilder.UI
{
    public class MenuFollowHead : MonoBehaviour
    {
        [SerializeField] private Transform head;
        [SerializeField] private float distance = 0.75f;
        [SerializeField] private float heightOffset = -0.5f;
        [SerializeField] private float sideOffset = 0.25f;
        [SerializeField] private bool followContinuously = true;
        [SerializeField] private float followSpeed = 8f;
        [SerializeField] private Vector3 rotationOffsetEuler = new Vector3(35f, 0f, 0f);

        private void OnEnable()
        {
            SnapInFrontOfUser();
        }

        private void Update()
        {
            if (followContinuously)
                MoveInFrontOfUser();
        }

        public void SnapInFrontOfUser()
        {
            if (head == null)
                return;

            GetTargetPose(out Vector3 targetPosition, out Quaternion targetRotation);

            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }

        private void MoveInFrontOfUser()
        {
            if (head == null)
                return;

            GetTargetPose(out Vector3 targetPosition, out Quaternion targetRotation);

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followSpeed);
        }

        private void GetTargetPose(out Vector3 targetPosition, out Quaternion targetRotation)
        {
            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;

            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            targetPosition = head.position + forward * distance + right * sideOffset + Vector3.up * heightOffset;

            Vector3 facingDirection = Vector3.ProjectOnPlane(targetPosition - head.position, Vector3.up).normalized;

            if (facingDirection.sqrMagnitude < 0.001f)
                facingDirection = forward;

            Quaternion baseRotation = Quaternion.LookRotation(facingDirection, Vector3.up);

            targetRotation = baseRotation * Quaternion.Euler(rotationOffsetEuler);
        }

        public void ToggleFollowHead()
        {
            followContinuously = !followContinuously;
        }
    }
}