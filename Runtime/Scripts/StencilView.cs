using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Indominuspsych.PolySpatialStencilMask
{
    public class StencilView : MonoBehaviour
    {
        [Serializable]
        public class StencilProjectionType
        {
            public GameObject staticStencil;
            public GameObject dynamicStencil;
        }

        [Serializable]
        public class DynamicStencilInfo
        {
            public Image counterIndicator;
            public TMP_Text counter;
        }

        [Serializable]
        public class StencilSetupInfo
        {
            public Material materialStencil;
            public Transform stencilMask;
            public Transform stencilObject;
        }

        [SerializeField]
        private StencilProjectionType stencilProjectionType;

        [SerializeField]
        private DynamicStencilInfo dynamicStencilInfo;

        [SerializeField]
        private StencilSetupInfo stencilSetupInfo;

        [SerializeField]
        private bool showBoundaryGizmo = true;

        [SerializeField]
        private Color boundaryGizmoColor = new (1f, 0.1f, 0.5f, 0.35f);

        // Boundary point set now authored as a StencilBoundary asset (see that
        // class for the meters/axis convention and the scale-factor writeup) so it
        // can be edited/added-to from the Editor instead of hardcoded here. Drag
        // the asset matching the desired shape into this field.
        [SerializeField]
        private StencilBoundary activeBoundary;

        private int counterValue = 0;
        private float elapsed = 0f;

        void Start()
        {
            SetShaderValues();
            ActivateStencil(stencilProjectionType.staticStencil);
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            DynamicStencil();
        }

        private void DynamicStencil()
        {
            if (stencilProjectionType.dynamicStencil != null)
            {
                if (stencilProjectionType.dynamicStencil.activeSelf)
                {
                    if (counterValue < 100)
                    {
                        if (elapsed >= 0.2f)
                        {
                            elapsed %= 0.2f;
                            counterValue += 1;
                            DynmaicStencilUpdation();
                        }
                    }
                    else
                    {
                        counterValue = 0;
                        elapsed = 0f;
                        if (dynamicStencilInfo.counterIndicator != null)
                        {
                            dynamicStencilInfo.counterIndicator.fillAmount = 0;
                        }
                    }
                }
            }
        }

        private void SetShaderValues()
        {
            if (stencilProjectionType.dynamicStencil.activeSelf || stencilProjectionType.staticStencil.activeSelf)
            {
                if (stencilSetupInfo.stencilMask != null && stencilSetupInfo.materialStencil != null)
                {
                    stencilSetupInfo.materialStencil.SetVector("_StencilMaskNormal", stencilSetupInfo.stencilMask.TransformDirection(Vector3.up));
                    stencilSetupInfo.materialStencil.SetVector("_StencilMaskRight", stencilSetupInfo.stencilMask.TransformDirection(Vector3.right));
                    stencilSetupInfo.materialStencil.SetVector("_StencilMaskUp", stencilSetupInfo.stencilMask.TransformDirection(Vector3.forward));
                    stencilSetupInfo.materialStencil.SetVector("_StencilMaskCenter", stencilSetupInfo.stencilMask.transform.position);
                    stencilSetupInfo.materialStencil.SetVector("_StencilMaskSize", new Vector2(stencilSetupInfo.stencilMask.localScale.x, stencilSetupInfo.stencilMask.localScale.z));
                    stencilSetupInfo.materialStencil.SetVector("_ScaleFactor", new Vector2(1f / stencilSetupInfo.stencilMask.localScale.x, 1f / stencilSetupInfo.stencilMask.localScale.z));
                    stencilSetupInfo.materialStencil.SetMatrix("_StencilMaskWorldToLocal", stencilSetupInfo.stencilMask.worldToLocalMatrix);
                    SetBoundaryForShader();
                }
            }
        }

        private void SetBoundaryForShader()
        {
            if (activeBoundary == null)
            {
                Debug.LogWarning("StencilView: no Active Boundary assigned, skipping boundary upload.");
                return;
            }

            if (activeBoundary.points.Count != 6)
            {
                Debug.LogWarning($"StencilView: '{activeBoundary.name}' has {activeBoundary.points.Count} points, but the shader expects exactly 6 (_Point0.._Point5).");
            }

            int index = 0;
            foreach (var point in activeBoundary.points)
            {
                stencilSetupInfo.materialStencil.SetVector($"_Point{index}", point);
                index += 1;
            }
        }

        // Editor-only preview of the currently active boundary polygon, drawn directly
        // in world space on stencilMask (right = Point.x, forward = Point.y - same
        // convention documented on StencilBoundary). Independent of the shader's
        // _ScaleFactor/_StencilMaskWorldToLocal path, so it doubles as a sanity check that
        // doesn't rely on the mesh's own vertex extents at all.
        private void OnDrawGizmos()
        {
            if (!showBoundaryGizmo || stencilSetupInfo == null || stencilSetupInfo.stencilMask == null || activeBoundary == null)
            {
                return;
            }

            var points = activeBoundary.points;
            if (points == null || points.Count == 0)
            {
                return;
            }

            Transform glass = stencilSetupInfo.stencilMask;
            var worldPoints = new Vector3[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                worldPoints[i] = glass.position + glass.right * points[i].x + glass.forward * points[i].y;
            }

#if UNITY_EDITOR
            UnityEditor.Handles.color = boundaryGizmoColor;
            UnityEditor.Handles.DrawAAConvexPolygon(worldPoints);
#endif

            Gizmos.color = new Color(boundaryGizmoColor.r, boundaryGizmoColor.g, boundaryGizmoColor.b, 1f);
            for (int i = 0; i < worldPoints.Length; i++)
            {
                Gizmos.DrawLine(worldPoints[i], worldPoints[(i + 1) % worldPoints.Length]);
            }
        }

        private void DynmaicStencilUpdation()
        {
            if (dynamicStencilInfo.counter != null && dynamicStencilInfo.counterIndicator != null)
            {
                dynamicStencilInfo.counter.text = $"{counterValue}";
                dynamicStencilInfo.counterIndicator.fillAmount = counterValue / 99f;
                if (counterValue < 30)
                {
                    dynamicStencilInfo.counterIndicator.color = new Color32(255, 255, 255, 255);
                }
                else if (counterValue >= 30 && counterValue < 60)
                {
                    dynamicStencilInfo.counterIndicator.color = new Color32(0, 255, 0, 255);
                }
                else if (counterValue >= 60 && counterValue <= 99)
                {
                    dynamicStencilInfo.counterIndicator.color = new Color32(255, 0, 0, 255);
                }
            }
        }

        private void HideStencils()
        {
            stencilProjectionType.dynamicStencil.SetActive(false);
            stencilProjectionType.staticStencil.SetActive(false);
        }

        public void ActivateStencil(GameObject gameObject)
        {
            HideStencils();
            gameObject.SetActive(true);
            SetShaderValues();
        }
    }
}
