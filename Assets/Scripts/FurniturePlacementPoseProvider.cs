using UnityEngine;

namespace RoomBuilder.Runtime
{
    public class FurniturePlacementPoseProvider : MonoBehaviour
    {
        [Header("Head Source")]
        [SerializeField] private Transform head;

        [Header("Placement")]
        [SerializeField] private float spawnDistance = 1.5f;
        [SerializeField] private float groundY = 0f;

        public bool TryGetPlacementPose(out Pose pose)
        {
            if (head == null)
            {
                pose = default;
                return false;
            }

            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;

            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;

            Vector3 spawnPosition = head.position + forward * spawnDistance;
            spawnPosition.y = groundY;

            Quaternion spawnRotation = Quaternion.LookRotation(forward, Vector3.up);

            pose = new Pose(spawnPosition, spawnRotation);
            return true;
        }
    }
}