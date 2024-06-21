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
    }
}