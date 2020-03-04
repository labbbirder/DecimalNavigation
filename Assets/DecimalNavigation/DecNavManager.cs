using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DecimalNavigation
{
    public class DecNavManager : MonoBehaviour
    {
        public NormalizedNavmeshAsset navMesh;
        NavigationSystem _system;
        public NavigationSystem system
        {
            get
            {
                _system = _system ?? new NavigationSystem(navMesh);
                return _system;
            }
        }
        private void OnValidate()
        {
            navMesh = NormalizedNavmeshAsset.GetInstanceOfCurrentScene();
        }
    }
}