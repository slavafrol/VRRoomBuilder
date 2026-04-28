using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoomBuilder.Runtime
{
    public class FurnitureSelectionHighlight : MonoBehaviour
    {
        [Header("Outline")]
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private Color outlineColor = new Color(0.2f, 0.8f, 1f, 1f);
        [SerializeField] private float outlineWidth = 0.025f;

        private readonly List<Renderer> outlineRenderers = new List<Renderer>();
        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            BuildOutline();

            SetHighlighted(false);
        }

        public void SetHighlighted(bool highlighted)
        {
            if (highlighted && outlineRenderers.Count == 0)
                BuildOutline();

            foreach (Renderer outlineRenderer in outlineRenderers)
            {
                if (outlineRenderer != null)
                    outlineRenderer.enabled = highlighted;
            }
        }

        private void BuildOutline()
        {
            if (outlineMaterial == null)
            {
                Debug.LogWarning($"{name}: outline material is missing.");
                return;
            }

            MeshRenderer[] sourceRenderers = GetComponentsInChildren<MeshRenderer>(true);

            foreach (MeshRenderer sourceRenderer in sourceRenderers)
            {
                if (sourceRenderer == null)
                    continue;

                if (sourceRenderer.name.EndsWith("_Outline"))
                    continue;

                MeshFilter sourceMeshFilter = sourceRenderer.GetComponent<MeshFilter>();

                if (sourceMeshFilter == null || sourceMeshFilter.sharedMesh == null)
                    continue;

                GameObject outlineObject = new GameObject(sourceRenderer.name + "_Outline");

                outlineObject.transform.SetParent(sourceRenderer.transform, false);
                outlineObject.transform.localPosition = Vector3.zero;
                outlineObject.transform.localRotation = Quaternion.identity;
                outlineObject.transform.localScale = Vector3.one;

                MeshFilter outlineMeshFilter = outlineObject.AddComponent<MeshFilter>();
                outlineMeshFilter.sharedMesh = sourceMeshFilter.sharedMesh;

                MeshRenderer outlineMeshRenderer = outlineObject.AddComponent<MeshRenderer>();

                int materialCount = Mathf.Max(1, sourceMeshFilter.sharedMesh.subMeshCount);
                Material[] outlineMaterials = new Material[materialCount];

                for (int i = 0; i < outlineMaterials.Length; i++)
                    outlineMaterials[i] = outlineMaterial;

                outlineMeshRenderer.sharedMaterials = outlineMaterials;

                outlineMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                outlineMeshRenderer.receiveShadows = false;
                outlineMeshRenderer.lightProbeUsage = LightProbeUsage.Off;
                outlineMeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

                ApplyOutlineProperties(outlineMeshRenderer);

                outlineMeshRenderer.enabled = false;
                outlineRenderers.Add(outlineMeshRenderer);
            }
        }

        private void ApplyOutlineProperties(Renderer outlineRenderer)
        {
            if (outlineRenderer == null)
                return;

            propertyBlock.Clear();

            propertyBlock.SetColor("_OutlineColor", outlineColor);
            propertyBlock.SetFloat("_OutlineWidth", outlineWidth);

            outlineRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}