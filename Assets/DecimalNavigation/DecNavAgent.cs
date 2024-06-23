using System.Collections;
using System.Collections.Generic;
using FixMath.NET;
using UnityEngine;
using UnityEngine.Profiling;
using static DecimalNavigation.NavigationSystem;

namespace DecimalNavigation
{

    public class DecNavAgent : MonoBehaviour
    {
        public int precision => manager.navMesh.precision;
        [Range(10, 120)]
        [Tooltip("逻辑层帧率")]
        public int FramesPerSecond = 30;
        DecNavManager manager;
        /// <summary>
        /// 逻辑层速度，每秒的行进逻辑距离
        /// </summary>
        [Tooltip("每秒前进的逻辑距离")]
        public int speed = 400;
        [Tooltip("渲染时紧贴地面，不影响逻辑层")]
        public bool groundCast = false;
        /// <summary>
        /// 逻辑层的目的地
        /// </summary>
        public Point2D destination { get; private set; }
        /// <summary>
        /// 逻辑层的当前位置
        /// </summary>
        public Point2D localtion { get; private set; }
        public List<Point2D> path;
        private int coveredLength;
        // Start is called before the first frame update
        void Awake()
        {
            manager = FindObjectOfType<DecNavManager>();
        }

        public void SetDestination(Point2D dest)
        {
            destination = dest;
            coveredLength = 0;
            path.Clear();
            manager.system.CalculatePath(localtion, destination, path);
        }

        public void SetLocation(Point2D loc)
        {
            localtion = loc;
            destination = loc;
            coveredLength = 0;
            path = new();
        }
        /// <summary>
        /// 每帧调用
        /// </summary>
        public void UpdateOnce(bool updateTransform = true)
        {
            coveredLength += speed / FramesPerSecond;
            Fix64 len = 0;
            if (path.Count >= 2)
                for (int i = 1; i < path.Count; i++)
                {
                    var secLen = (path[i] - path[i - 1]).Magnitude;
                    if (len + secLen > coveredLength)
                    {
                        localtion = path[i - 1] + (path[i] - path[i - 1]) * (coveredLength - len) / secLen;
                        break;
                    }
                    len += secLen;
                }
            if (updateTransform)
            {
                // var pos = localtion.ToVector3() / manager.navMesh.precision;


                // if (groundCast)
                // {
                //     RaycastHit hit;
                //     if (Physics.Raycast(new Ray(pos + Vector3.up * 9999, Vector3.down), out hit, 99999, LayerMask.GetMask("ground")))
                //     {
                //         pos.y = hit.point.y;
                //     }
                // }
                // transform.position = pos;
            }
        }
        private void FixedUpdate()
        {
            UpdateOnce(true);
        }
    }
}
