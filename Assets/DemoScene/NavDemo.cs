using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using DecimalNavigation;
using System.Diagnostics;
using static DecimalNavigation.NavigationSystem;
using UnityEngine.Profiling;

public class NavDemo : MonoBehaviour
{
    DecNavAgent agent;
    //// Start is called before the first frame update
    void Start()
    {
        agent = FindObjectOfType<DecNavAgent>();
        agent.SetLocation(ToP2(agent.transform.position));
        agent.SetDestination(ToP2(agent.transform.position));
        // agent.SetLocation(new Point3D(
        //     new Vector3(-0.699999988f, 0.0839999989f, 2.43000007f) * agent.precision
        // ));
        // agent.SetDestination(agent.localtion);
    }
    public List<Point2D> path = new();


    private void OnDrawGizmos()
    {
        if (null == path) return;
        Gizmos.color = Color.blue;
        for (int i = 1; i < path.Count; i++)
        {
            Gizmos.DrawLine(ToV3(path[i - 1]), ToV3(path[i]));
        }
    }

    Vector3 ToV3(Point2D p2)
    {
        return new Vector3(p2.X, 0, p2.Y) / agent.precision;
    }
    Point2D ToP2(Vector3 v3)
    {
        v3 *= agent.precision;
        return new Point2D((long)v3.x, (long)v3.z);
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

                var p2 = ToP2(p);
                agent.SetDestination(p2);

                path = agent.path;
                print(path.Count);
                Profiler.BeginSample("dec agent");
                for (int i = 0; i < 1000; i++)
                {
                    agent.SetDestination(p2);
                }
                Profiler.EndSample();

                var ua = GetComponent<NavMeshAgent>();
                NavMeshPath up = new NavMeshPath();
                Profiler.BeginSample("uni agent");
                for (int i = 0; i < 1000; i++)
                {
                    ua.CalculatePath(p, up);
                }
                Profiler.EndSample();
            }

        }
        if (Input.GetMouseButtonDown(1))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var p = hit.point;
                // agent.SetLocation(new Point3D(p * agent.precision));s
            }
        }
    }
}
