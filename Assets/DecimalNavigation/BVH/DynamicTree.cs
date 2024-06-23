/*
 * Copyright (c) Thorben Linneweber and others
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using DecimalNavigation;
using DeterministicMath;
using UnityEngine.Assertions;
using scalar = FixMath.NET.Fix64;

namespace Jitter2.Collision
{
    /// <summary>
    /// Represents a dynamic Axis Aligned Bounding Box (AABB) tree. A hashset (refer to <see cref="PairHashSet"/>)
    /// maintains a record of potential overlapping pairs.
    /// </summary>
    /// <typeparam name="T">The type of elements stored in the dynamic tree.</typeparam>
    public partial class DynamicTree<T> where T : class, IDynamicTreeProxy
    {
        public const int NullNode = -1;
        public const int InitialSize = 1024;

        /// <summary>
        /// Represents a node in the AABB tree.
        /// </summary>
        public struct Node
        {
            public int Left, Right;
            public int Parent;
            public int Height;

            public AABB2D ExpandedBox;
            public T Proxy;

            public bool IsLeaf
            {
                readonly get => Left == NullNode;
                set => Left = value ? NullNode : Left;
            }

        }

        public Node[] Nodes = new Node[InitialSize];
        private readonly Stack<int> freeNodes = new();
        private int nodePointer = -1;
        private int root = NullNode;

        /// <summary>
        /// Gets the root of the dynamic tree.
        /// </summary>
        public int Root => root;

        /// <summary>
        /// Updates the state of the specified entity within the dynamic tree structure.
        /// </summary>
        /// <param name="shape">The entity to update.</param>
        public void Update(T shape)
        {
            // OverlapCheck(shape, false);
            InternalRemoveProxy(shape);
            InternalAddProxy(shape);
            // OverlapCheck(shape, true);
        }

        /// <summary>
        /// Add an entity to the tree.
        /// </summary>
        public void AddProxy(T proxy)
        {
            InternalAddProxy(proxy);
            // OverlapCheck(root, proxy.NodePtr, true);
        }

        /// <summary>
        /// Removes an entity from the tree.
        /// </summary>
        public void RemoveProxy(T proxy)
        {
            // OverlapCheck(root, proxy.NodePtr, false);
            InternalRemoveProxy(proxy);
            proxy.NodePtr = NullNode;
        }

        /// <summary>
        /// Clears all entities from the tree.
        /// </summary>
        public void Clear()
        {
            nodePointer = -1;
            root = NullNode;
        }

        /// <summary>
        /// Calculates the cost function of the tree.
        /// </summary>
        /// <returns>The calculated cost.</returns>
        public scalar CalculateCost()
        {
            return Cost(ref Nodes[root]);
        }

        /// <summary>
        /// Calculates the height of the tree.
        /// </summary>
        /// <returns>The calculated height.</returns>
        public scalar CalculateHeight()
        {
            int calcHeight = Height(ref Nodes[root]);
            Assert.IsTrue(calcHeight == Nodes[root].Height);
            return calcHeight;
        }

        /// <summary>
        /// Enumerates all axis-aligned bounding boxes in the tree.
        /// </summary>
        /// <param name="action">The action to perform on each bounding box and node height in the tree.</param>
        public void EnumerateAll(Action<AABB2D, int> action)
        {
            if (root == -1) return;
            EnumerateAll(ref Nodes[root], action);
        }

        [ThreadStatic] private static Stack<int> stack;

        /// <summary>
        /// Queries the tree to find entities within the specified axis-aligned bounding box.
        /// </summary>
        /// <param name="hits">A list to store the entities found within the bounding box.</param>
        /// <param name="aabb">The axis-aligned bounding box used for the query.</param>
        public void Query(List<T> hits, in AABB2D aabb)
        {
            stack ??= new Stack<int>(256);

            stack.Push(root);

            while (stack.Count > 0)
            {
                int index = stack.Pop();

                Node node = Nodes[index];

                if (node.IsLeaf)
                {
                    hits.Add(node.Proxy);
                }
                else
                {
                    int child1 = Nodes[index].Left;
                    int child2 = Nodes[index].Right;

                    if (Nodes[child1].ExpandedBox.NotDisjoint(aabb))
                        stack.Push(child1);

                    if (Nodes[child2].ExpandedBox.NotDisjoint(aabb))
                        stack.Push(child2);
                }
            }

            stack.Clear();
        }
        public void Query(List<T> hits, in Point2D point)
        {
            stack ??= new Stack<int>(256);

            stack.Push(root);

            while (stack.Count > 0)
            {
                int index = stack.Pop();

                Node node = Nodes[index];

                if (node.IsLeaf)
                {
                    hits.Add(node.Proxy);
                }
                else
                {
                    int child1 = Nodes[index].Left;
                    int child2 = Nodes[index].Right;

                    if (Nodes[child1].ExpandedBox.Contains(point))
                        stack.Push(child1);

                    if (Nodes[child2].ExpandedBox.Contains(point))
                        stack.Push(child2);
                }
            }

            stack.Clear();
        }

        private int AllocateNode()
        {
            if (freeNodes.Count > 0)
            {
                return freeNodes.Pop();
            }

            nodePointer += 1;
            if (nodePointer == Nodes.Length)
            {
                Array.Resize(ref Nodes, Nodes.Length * 2);
                UnityEngine.Debug.LogWarning($"Resized array of AABBTree to {Nodes.Length} elements.");
            }

            return nodePointer;
        }

        private void FreeNode(int node)
        {
            freeNodes.Push(node);
        }

        private scalar Cost(ref Node node)
        {
            if (node.IsLeaf)
            {
                // Assert.IsTrue(node.ExpandedBox.Perimeter < 1e8);
                return node.ExpandedBox.Perimeter;
            }

            return node.ExpandedBox.Perimeter + Cost(ref Nodes[node.Left]) + Cost(ref Nodes[node.Right]);
        }

        private int Height(ref Node node)
        {
            if (node.IsLeaf) return 0;
            return 1 + Math.Max(Height(ref Nodes[node.Left]), Height(ref Nodes[node.Right]));
        }

        private void EnumerateAll(ref Node node, Action<AABB2D, int> action, int depth = 0)
        {
            action(node.ExpandedBox, depth);
            if (node.IsLeaf) return;

            EnumerateAll(ref Nodes[node.Left], action, depth + 1);
            EnumerateAll(ref Nodes[node.Right], action, depth + 1);
        }

        private void InternalAddProxy(T proxy)
        {
            AABB2D b = proxy.WorldBoundingBox;

            int index = AllocateNode();

            Nodes[index].Proxy = proxy;
            Nodes[index].IsLeaf = true;
            Nodes[index].Height = 0;
            proxy.NodePtr = index;

            Nodes[index].ExpandedBox = b;

            AddLeaf(index);
        }

        private void InternalRemoveProxy(T proxy)
        {
            Assert.IsTrue(Nodes[proxy.NodePtr].IsLeaf);

            RemoveLeaf(proxy.NodePtr);
            FreeNode(proxy.NodePtr);
        }

        private void RemoveLeaf(int node)
        {
            if (node == root)
            {
                root = NullNode;
                return;
            }

            int parent = Nodes[node].Parent;
            int grandParent = Nodes[parent].Parent;
            int sibling;

            if (Nodes[parent].Left == node) sibling = Nodes[parent].Right;
            else sibling = Nodes[parent].Left;

            if (grandParent != NullNode)
            {
                if (Nodes[grandParent].Left == parent) Nodes[grandParent].Left = sibling;
                else Nodes[grandParent].Right = sibling;

                Nodes[sibling].Parent = grandParent;
                FreeNode(parent);

                int index = grandParent;
                while (index != NullNode)
                {
                    int left = Nodes[index].Left;
                    int rght = Nodes[index].Right;

                    AABB2D.CreateMerged(Nodes[left].ExpandedBox, Nodes[rght].ExpandedBox, out Nodes[index].ExpandedBox);
                    Nodes[index].Height = 1 + Math.Max(Nodes[left].Height, Nodes[rght].Height);
                    index = Nodes[index].Parent;
                }
            }
            else
            {
                root = sibling;
                Nodes[sibling].Parent = NullNode;
                FreeNode(parent);
            }
        }

        private static scalar MergedPerimeter(in AABB2D box1, in AABB2D box2)
        {
            scalar a, b;
            scalar x, y;

            a = box1.Min.X < box2.Min.X ? box1.Min.X : box2.Min.X; //min x
            b = box1.Max.X > box2.Max.X ? box1.Max.X : box2.Max.X; //max x

            x = b - a; // x len

            a = box1.Min.Y < box2.Min.Y ? box1.Min.Y : box2.Min.Y; //min y
            b = box1.Max.Y > box2.Max.Y ? box1.Max.Y : box2.Max.Y; //max y

            y = b - a; // y len

            // a = box1.Min.z < box2.Min.z ? box1.Min.z : box2.Min.z; //min z
            // b = box1.Max.z > box2.Max.z ? box1.Max.z : box2.Max.z; //max z

            // z = b - a; // z len
            return x * y;
        }

        private void AddLeaf(int node)
        {
            if (root == NullNode)
            {
                root = node;
                Nodes[root].Parent = NullNode;
                return;
            }

            // search for the best sibling
            // int sibling = root;
            AABB2D nodeBox = Nodes[node].ExpandedBox;

            int sibling = root;

            while (!Nodes[sibling].IsLeaf)
            {
                int left = Nodes[sibling].Left;
                int rght = Nodes[sibling].Right;

                scalar area = Nodes[sibling].ExpandedBox.Perimeter;

                scalar combinedArea = MergedPerimeter(Nodes[sibling].ExpandedBox, nodeBox);

                scalar cost = 2 * combinedArea;
                scalar inhcost = 2 * (combinedArea - area);
                scalar costl, costr;

                if (Nodes[left].IsLeaf)
                {
                    costl = inhcost + MergedPerimeter(Nodes[left].ExpandedBox, nodeBox);
                }
                else
                {
                    scalar oldArea = Nodes[left].ExpandedBox.Perimeter;
                    scalar newArea = MergedPerimeter(Nodes[left].ExpandedBox, nodeBox);
                    costl = newArea - oldArea + inhcost;
                }

                if (Nodes[rght].IsLeaf)
                {
                    costr = inhcost + MergedPerimeter(Nodes[rght].ExpandedBox, nodeBox);
                }
                else
                {
                    scalar oldArea = Nodes[rght].ExpandedBox.Perimeter;
                    scalar newArea = MergedPerimeter(Nodes[rght].ExpandedBox, nodeBox);
                    costr = newArea - oldArea + inhcost;
                }

                // costl /= 2;
                // costr /= 2;

                // if this is true, the choice is actually the best for the current candidate
                if (cost < costl && cost < costr) break;

                sibling = costl < costr ? left : rght;
            }

            // create a new parent
            int oldParent = Nodes[sibling].Parent;
            int newParent = AllocateNode();

            Nodes[newParent].Parent = oldParent;
            Nodes[newParent].Height = Nodes[sibling].Height + 1;

            if (oldParent != NullNode)
            {
                if (Nodes[oldParent].Left == sibling) Nodes[oldParent].Left = newParent;
                else Nodes[oldParent].Right = newParent;

                Nodes[newParent].Left = sibling;
                Nodes[newParent].Right = node;
                Nodes[sibling].Parent = newParent;
                Nodes[node].Parent = newParent;
            }
            else
            {
                Nodes[newParent].Left = sibling;
                Nodes[newParent].Right = node;
                Nodes[sibling].Parent = newParent;
                Nodes[node].Parent = newParent;
                root = newParent;
            }

            int index = Nodes[node].Parent;
            while (index != NullNode)
            {
                int lft = Nodes[index].Left;
                int rgt = Nodes[index].Right;

                AABB2D.CreateMerged(Nodes[lft].ExpandedBox, Nodes[rgt].ExpandedBox, out Nodes[index].ExpandedBox);
                Nodes[index].Height = 1 + Math.Max(Nodes[lft].Height, Nodes[rgt].Height);
                index = Nodes[index].Parent;
            }
        }
    }
}