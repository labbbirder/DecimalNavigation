using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DecimalNavigation
{
    public class DecNavManager : MonoBehaviour
    {
        public NormalizedNavmeshAsset navMesh;
        NavigationSystem _system;
        public NavigationSystem system => _system ??= new NavigationSystem(navMesh);
        private void OnValidate()
        {
            if (!navMesh)
            {
                navMesh = NormalizedNavmeshAsset.GetInstanceOfCurrentScene();

            }
        }
        void OnDrawGizmosSelected()
        {
            if (navMesh?.polygons == null) return;
            var points = navMesh.points;
            var precision = navMesh.precision;
            foreach (var poly in navMesh.polygons)
            {
                var indices = poly.indices;
                for (int i = 0; i < indices.Length; i++)
                {
                    var a = points[indices[i]];
                    var b = points[indices[-~i % indices.Length]];
                    Gizmos.DrawLine(
                        new Vector3(a.X, 0, a.Y) / precision,
                        new Vector3(b.X, 0, b.Y) / precision);
                }
            }
        }
    }
}