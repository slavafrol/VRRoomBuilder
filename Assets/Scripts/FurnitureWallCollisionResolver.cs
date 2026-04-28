using UnityEngine;

namespace RoomBuilder.Runtime
{
    public class FurnitureWallCollisionResolver : MonoBehaviour
    {
        [SerializeField] private LayerMask wallMask;
        [SerializeField] private int solverIterations = 3;
        [SerializeField] private float skinWidth = 0.005f;

        private readonly Collider[] overlapBuffer = new Collider[32];
        private Collider[] furnitureColliders;

        private void Awake()
        {
            RefreshFurnitureColliders();
        }

        private void OnEnable()
        {
            RefreshFurnitureColliders();
        }

        private void LateUpdate()
        {
            ResolveWallPenetration();
        }

        public void RefreshFurnitureColliders()
        {
            furnitureColliders = GetComponentsInChildren<Collider>();
        }

        private void ResolveWallPenetration()
        {
            if (furnitureColliders == null || furnitureColliders.Length == 0)
                RefreshFurnitureColliders();

            for (int iteration = 0; iteration < solverIterations; iteration++)
            {
                bool movedThisIteration = false;

                foreach (Collider furnitureCollider in furnitureColliders)
                {
                    if (furnitureCollider == null)
                        continue;

                    if (!furnitureCollider.enabled)
                        continue;

                    Bounds bounds = furnitureCollider.bounds;

                    int hitCount = Physics.OverlapBoxNonAlloc(
                        bounds.center,
                        bounds.extents + Vector3.one * skinWidth,
                        overlapBuffer,
                        Quaternion.identity,
                        wallMask,
                        QueryTriggerInteraction.Ignore
                    );

                    for (int i = 0; i < hitCount; i++)
                    {
                        Collider wallCollider = overlapBuffer[i];

                        if (wallCollider == null)
                            continue;

                        if (!wallCollider.enabled)
                            continue;

                        if (wallCollider.transform.IsChildOf(transform))
                            continue;

                        bool isPenetrating = Physics.ComputePenetration(
                            furnitureCollider,
                            furnitureCollider.transform.position,
                            furnitureCollider.transform.rotation,
                            wallCollider,
                            wallCollider.transform.position,
                            wallCollider.transform.rotation,
                            out Vector3 direction,
                            out float distance
                        );

                        if (!isPenetrating)
                            continue;

                        Vector3 correction = direction * (distance + skinWidth);
                        correction.y = 0f;

                        if (correction.sqrMagnitude < 0.000001f)
                            continue;

                        transform.position += correction;
                        movedThisIteration = true;
                    }
                }

                if (!movedThisIteration)
                    break;
            }
        }
    }
}