# DecimalNavigation

基于定点数的寻路解决方案（实际使用的是 long 类型）, 适用于 unity 帧同步框架。

注意：寻路因为计算过程中的“离散”程度很高，输出的结果来自预先存储的浮点数，因此发生不同步的概率很小。只有极其苛刻的情况才会输出不同步的结果，只有当项目需要严格同步的情况才使用此库。

## TO DO List

基准测试效率比 Unity 自带算法快大约 30%~800%！

- [x] ~~优化：空间算法~~
- [ ] 继续优化：级联空间算法
- [ ] 优化：硬编码+实时计算
- [ ] 功能：网格编辑

## 为什么有用

在帧同步框架中，逻辑交由客户端自行判断，因此逻辑判断必须高度一致。

浮点数因以下原因无法保证多端同步：

- 舍入模式，编译时和运行时都可以指定，通常默认是 TO_NEAREST
- 扩展精度，在寄存器上保留额外精度，如 387 会为中间过程保留 80 位浮点而不是 64 位
- 组合指令，一些运算由专门运算器实现，可能会保留更多精度
- 目标指令集，如 sse2、387、软浮点等
- 解释器实现，IL 等情况则跟解释器的实现有关，甚至无法用编译指令调解
- 不符合规范的硬件设备，这种情况有，但可以忽略

而寻路是游戏中举足轻重的模块，市面上的主流寻路大多是 float 运算。

因此 DecimalNavigation 在运算层面选择了 long。

## 工作机制

### 开发时

DecimalNavigation 把 Unity 生成的 NavMesh 网格规格化另存。
数据加工流程：

- 提取寻路 Mesh 网格信息
- float 坐标放大后转 long
- 等距空间划分（未完成）
- 临点焊接（删除重合点，剔除无用三角形）
- 合并多边形（三角形列表转凸多边形列表）
- 序列化存储

btw,存储的精度是可配置的，默认为 100(100 分 1m)。

### 运行时

初始化流程：

- 加载序列化存储的信息
- BVH 空间重建、多边形关系初始化

寻路流程：

- 出入点规格化转结点
- A\*寻路得到结点列表（AStar Path Search）
- 拐角探测（Corner Probe）
- 开发者自行经过缩小坐标后从逻辑层传回表现层

## 快速入门

### 0.导入 DecimalNavigation

拷贝 Assets 下内容到自己的 Assets 下。

如果成功导入，菜单会出现`Tools/生成标准化寻路网格`。

### 1.得到网格

- 在场景下把地形标记为 static
- 使用 Unity 内置的寻路功能生成当前场景的 NavMesh
- 点击`Tools/生成标准化寻路网格`
- 在`Assets/NavMeshResource`中查看成果

### 2.网格精度修改\*

如果想要自己指定网格存储精度，可按如下步骤操作：

- 在`Assets/NavMeshResource`选择对应资源
- 修动 precision 值
- 右键菜单中点击`重新计算`

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
