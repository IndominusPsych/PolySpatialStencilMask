using System.Collections.Generic;
using UnityEngine;

namespace Indominuspsych.PolySpatialStencilMask
{
    // Boundary points below are authored in REAL-WORLD METERS, as an offset from
    // the reference plane's center: Point.x = meters along stencilMask's local
    // RIGHT axis, Point.y = meters along stencilMask's local FORWARD axis
    // (uploaded to the shader as "_StencilMaskUp" - a naming leftover, it is
    // actually .forward not .up).
    //
    // StencilView.SetShaderValues() (1f / stencilMask.localScale) converts
    // these meters into the same local space _StencilMaskWorldToLocal produces for the
    // fragment, via world = local * scale  =>  local = world / scale. This holds
    // no matter what the glass mesh's own vertex extents are - a 1-unit Quad
    // (edge at local +/-0.5) or Unity's built-in 10-unit "Plane" primitive (edge
    // at local +/-5) - because both sides of the shader's comparison (fragment
    // local position, and PointN * _ScaleFactor) are derived via that identical
    // "divide by localScale" relationship. Confirmed by a full node-by-node trace
    // of StencilMask.shadergraph (2026-09-04): there is no hidden x10/divide-by-10
    // constant anywhere in the graph.
    //
    // The shader currently hardcodes exactly _Point0.._Point5 - six points - so
    // this list must contain exactly 6 entries for the existing shader to work.
    [CreateAssetMenu(fileName = "New Stencil Boundary", menuName = "Stencil/Boundary Definition")]
    public class StencilBoundary : ScriptableObject
    {
        public List<Vector2> points = new();
    }
}
