# DecimalNavigation
基于定点数的寻路解决方案（实际使用的是long类型）, 适用于unity帧同步框架。

## 为什么有用
在帧同步框架中，逻辑交由客户端自行判断，因此逻辑判断必须高度一致。

浮点数因以下原因无法保证多端同步：
* 舍入模式，编译时和运行时都可以指定，通常默认是TO_NEAREST
* 扩展精度，在寄存器上保留额外精度
* 组合指令，一些运算由专门运算器实现，可能会保留更多精度
* 目标指令集，如sse2、387、软浮点等
* 解释器实现，IL等情况则跟解释器的实现有关，甚至无法用编译指令调解
* 不符合规范的硬件设备，这种情况有，但可以忽略

而寻路是游戏中举足轻重的模块，市面上的主流寻路大多是float运算。

因此DecimalNavigation在运算层面选择了long。


## 工作机制
### 开发时
DecimalNavigation把Unity生成的NavMesh网格规格化另存。
数据加工流程：
* 提取寻路Mesh网格信息
* 临点焊接
* float坐标放大后转long
* 存储

btw,存储的精度是可配置的，默认为100(100分1m)。
### 运行时
初始化流程：
* 加载规格化网格
* 边界检测
* 生成结点信息

寻路流程：
* 出入点规格化转结点
* A*寻路得到结点列表（AStar Path Search）
* 拐角探测（Corner Probe）
* ~~脊背切割（Ridge Cut）~~
* 开发者自行经过缩小坐标后从逻辑层传回表现层

## 快速入门
### 0.导入DecimalNavigation
拷贝Assets下内容到自己的Assets下。

如果成功导入，菜单会出现`Tools/生成标准化寻路网格`。
### 1.得到网格
* 在场景下把地形标记为static
* 使用Unity内置的寻路功能生成当前场景的NavMesh
* 点击`Tools/生成标准化寻路网格`
* 在`Assets/NavMeshResource`中查看成果
### 2.网格精度修改*
如果想要自己指定网格存储精度，可按如下步骤操作：
* 在`Assets/NavMeshResource`选择对应资源
* 修动precision值
* 右键菜单中点击`重新计算`
### 3.开始寻路
引用命名空间
```CSharp
using DecimalNavigation;
```
初始化时，新建一个寻路实例
```CSharp
//在Inspector中指定上一步得到的成果
public NormalizedNavmeshAsset navMesh;

private NavigationSystem system;

void Start(){
  system = new NavigationSystem(navMesh);
}
```
点击地面，得到路线
```CSharp
List<Point3D> path;

private void Update()
{
    if (Input.GetMouseButton(0))
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var p = hit.point;
            path = system.CalculatePath(
                new Point3D(transform.position * navMesh.precision), 
                new Point3D(p * navMesh.precision)
            );
        }
    }
}
```
绘制路径折线，便于观察
```CSharp

private void OnDrawGizmos()
{
    if (null == path) return;
    Gizmos.color = Color.blue;
    for (int i = 1; i < path.Count; i++)
    {
        Gizmos.DrawLine(
            path[i - 1].toVector3() / navMesh.precision, 
            path[i].toVector3() / navMesh.precision
        );
    }
}
```
## 写在最后
这个框架我会随着工作不断完善，不断优化。感谢期待，也感谢好的建议。
