using UnityEngine;

namespace RoomBuilder.Runtime
{
    public class FurnitureScaleLimiter : MonoBehaviour
    {
        [SerializeField] private float minUniformScale = 0.5f;
        [SerializeField] private float maxUniformScale = 2f;

        public void Configure(float minScale, float maxScale)
        {
            minUniformScale = Mathf.Max(0.01f, minScale);
            maxUniformScale = Mathf.Max(minUniformScale, maxScale);

            ClampCurrentVectorScale();
        }

        public void ScaleBy(float multiplier)
        {
            Vector3 desiredScale = transform.localScale * multiplier;
            SetScaleClamped(desiredScale);
        }

        public void SetUniformScale(float scale)
        {
            float clamped = Mathf.Clamp(scale, minUniformScale, maxUniformScale);
            transform.localScale = new Vector3(clamped, clamped, clamped);
        }

        public Vector3 ClampScale(Vector3 desiredScale)
        {
            return new Vector3(
                Mathf.Clamp(desiredScale.x, minUniformScale, maxUniformScale),
                Mathf.Clamp(desiredScale.y, minUniformScale, maxUniformScale),
                Mathf.Clamp(desiredScale.z, minUniformScale, maxUniformScale)
            );
        }

        public void SetScaleClamped(Vector3 desiredScale)
        {
            transform.localScale = ClampScale(desiredScale);
        }

        private void ClampCurrentVectorScale()
        {
            transform.localScale = ClampScale(transform.localScale);
        }
    }
}