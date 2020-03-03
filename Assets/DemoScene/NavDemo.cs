using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using DecimalNavigation;
public class NavDemo : MonoBehaviour
{
    //AStarSystem system;
    public NormalizedNavmeshAsset navMesh;
    NavigationSystem navigationSystem;
    //public int prec = 100;
    //public Mesh mesh;
    //AStarSystem ast;
    //// Start is called before the first frame update
    void Start()
    {
        navigationSystem = new NavigationSystem(navMesh);
    }
    List<Point3D> path;

    private void OnDrawGizmosSelected()
    {
        if (navigationSystem == null) return;
        Gizmos.color = new Color32(255, 255, 255, 32);
        var vts = navMesh.vertices;
        var ids = navMesh.indices;
        for (int i = 0; i < ids.Length; i += 3)
        {
            Gizmos.DrawLine(vts[ids[i + 0]].ToVector3() / navMesh.precision, vts[ids[i + 1]].ToVector3() / navMesh.precision);
            Gizmos.DrawLine(vts[ids[i + 0]].ToVector3() / navMesh.precision, vts[ids[i + 2]].ToVector3() / navMesh.precision);
            Gizmos.DrawLine(vts[ids[i + 1]].ToVector3() / navMesh.precision, vts[ids[i + 2]].ToVector3() / navMesh.precision);
        }
        Gizmos.color = Color.green;
        for (int i = 0; i < navigationSystem.edges.Length; i += 2)
        {
            Gizmos.DrawLine(vts[navigationSystem.edges[i + 0]].ToVector3() / navMesh.precision, vts[navigationSystem.edges[i + 1]].ToVector3() / navMesh.precision);
        }
    }

    private void OnDrawGizmos()
    {
        if (null == path) return;
        Gizmos.color = Color.blue;
        for (int i = 1; i < path.Count; i++)
        {
            Gizmos.DrawLine(path[i - 1].ToVector3() / navMesh.precision, path[i].ToVector3() / navMesh.precision);
        }
        //Gizmos.color = Color.yellow;
        //var npath = navigationSystem.npath;
        //for (int i = 1; i < npath.Count; i++)
        //{
        //    Gizmos.DrawLine(npath[i - 1].ToVector3() / navMesh.precision, npath[i].ToVector3() / navMesh.precision);
        //}
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var p = hit.point;
                path = navigationSystem.CalculatePath(new Point3D(transform.position * navMesh.precision), new Point3D(p * navMesh.precision),enableRidgeCutting:true);
            }
        }
    }
}
