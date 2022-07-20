using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using DecimalNavigation;
using System.Diagnostics;

public class NavDemo : MonoBehaviour
{
    DecNavAgent agent;
    NavMeshAgent unityAgent;
    //// Start is called before the first frame update
    void Start()
    {
        agent = FindObjectOfType<DecNavAgent>();
        unityAgent = FindObjectOfType<NavMeshAgent>();
        //navigationSystem = new NavigationSystem(navMesh);
    }
    public Point3D[] path;


    private void OnDrawGizmos()
    {
        if (null == path) return;
        Gizmos.color = Color.blue;
        for (int i = 1; i < path.Length; i++)
        {
            Gizmos.DrawLine(path[i - 1].ToVector3() / agent.precision, path[i].ToVector3() / agent.precision);
        }
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


                var max = 100;
                Stopwatch watch = new Stopwatch();
                watch.Start();
                for (int i = 0; i < max; i++)
                {
                    unityAgent.CalculatePath(p, unityAgent.path);
                }
                print($"unityAgent:{watch.ElapsedMilliseconds}ms");

                watch.Restart();
                for (int i = 0; i < max; i++)
                {
                    agent.SetDestination(new Point3D(p * agent.precision));
                }
                print($"decimalAgent:{watch.ElapsedMilliseconds}ms");

                path = agent.path;
            }

        }
        if (Input.GetMouseButtonDown(1))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var p = hit.point;
                agent.SetLocation(new Point3D(p * agent.precision));
            }
        }
    }
}
