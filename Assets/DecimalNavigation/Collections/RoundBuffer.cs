using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace com.bbbirder.Collections
{
    public class RoundBuffer<T> : IList<T>
    {
        private T[] array;
        private int header;
        private int _sizeMinusOne;
        public int Count { get; private set; }

        public bool IsReadOnly => false;

        public T this[int index] { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public int IndexOf(T item)
        {
            for (int i = 0; i < Count; i++)
            {
                var idx = (i + header) & _sizeMinusOne;
                if (EqualityComparer<T>.Default.Equals(array[idx], item))
                {
                    return i;
                }
            }
            return -1;
        }
        public void EnsureSize(int size)
        {
            if (array.Length >= size) return;
            size = 1 << CollectionUtility.CeilExponent(size);
            _sizeMinusOne = size - 1;

            var nArr = new T[size];

            Array.Resize(ref array, size);
        }
        private void InternalPrepend(T item)
        {
            EnsureSize(++Count);
            header = ~-header & _sizeMinusOne;
            array[header] = item;
        }
        private void InternalAppend(T item)
        {
            EnsureSize(++Count);
            array[(header + Count) & _sizeMinusOne] = item;
        }

        public void Insert(int index, T item)
        {
            EnsureSize(++Count);
            header = ~-header & _sizeMinusOne;
            var end = header + index;
            var i = header;
            for (; i < end; i++)
            {
                array[i & _sizeMinusOne] = array[-~i & _sizeMinusOne];
            }
            array[i & _sizeMinusOne] = item;
        }

        public void RemoveAt(int index)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {

            }
        }

        public void Add(T item)
        {
            InternalAppend(item);
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}