using System;
using System.Runtime.CompilerServices;

namespace Core.Collections
{
    public abstract class KeySetItem
    {
        private int m_KeyIndex;
        internal object set;

        public int KeyIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_KeyIndex;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal set => m_KeyIndex = value;
        }
    }

    public partial class KeySet<T> : IDisposable where T : KeySetItem
    {
        private T[] m_Buffer;
        private int m_Count;
        private int m_Version;

        internal T[] Buffer => m_Buffer;
        internal int Version => m_Version;

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Count;
        }

        public T[] ToArray()
        {
            T[] array = new T[m_Count];
            System.Buffer.BlockCopy(m_Buffer, 0, array, 0, m_Count);
            return array;
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get 
            {
                if (index >= 0 && index < m_Count)
                {
                    return m_Buffer[index];
                }

                throw new IndexOutOfRangeException();
            }
        }

        public KeySet() : this(16)
        {
        }

        public KeySet(int capacity)
        {
            if (capacity < 1)
            {
                throw new Exception("capacity must > 0");
            }

            m_Buffer = new T[capacity];
            m_Count = 0;
            m_Version = 0;
        }

        ~KeySet()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool isDispose)
        {
            if (isDispose)
            {
                this.Clear();
                GC.SuppressFinalize(this);
            }
        }

        public void Clear()
        {
            if (m_Count > 0)
            {
                for (int i = 0; i < m_Count; ++i)
                {
                    m_Buffer[i].set = null;
                    m_Buffer[i] = null;
                }

                //Array.Clear(m_Buffer, 0, m_Count);
                m_Count = 0;
                ++m_Version;
            }
        }

        internal void ClearForDictionary()
        {
            Array.Clear(m_Buffer, 0, m_Count);
            m_Count = 0;
            ++m_Version;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckCapacity(int capacity)
        {
            if (capacity > m_Buffer.Length)
            {
                Array.Resize(ref m_Buffer, m_Buffer.Length * 2);
            }
        }

        public void Add(T item)
        {
            if (item == null)
            {
                throw new NullReferenceException();
            }

            if (item.set == null)
            {
                this.CheckCapacity(m_Count + 1);

                item.set = this;
                item.KeyIndex = m_Count;
                m_Buffer[m_Count++] = item;
                ++m_Version;
            }
            else if (item.set == this)
            {
                throw new Exception("the item add has already added");
            }
            else if (item.set != this)
            {
                throw new Exception("the item add has already added to another set");
            }
        }

        public void Insert(T at, T item)
        {
            if (item == null)
            {
                throw new NullReferenceException();
            }
            
            if (!this.Contains(at))
            {
                throw new Exception("KeySet.Insert at is not belong to set");
            }
            
            if (item.set == null)
            {
                this.InsertAtIndexNoCheck(at.KeyIndex, item);
            }
            else if (item.set == this)
            {
                throw new Exception("the item insert has already added");
            }
            else if (item.set != this)
            {
                throw new Exception("the item insert has already added to another set");
            }
        }
        public void InsertAfter(T at, T item)
        {
            if (item == null)
            {
                throw new NullReferenceException();
            }
            
            if (!this.Contains(at))
            {
                throw new Exception("KeySet.InsertAfter at is not belong to set");
            }
            
            if (item.set == null)
            {
                this.InsertAtIndexNoCheck(at.KeyIndex + 1, item);
            }
            else if (item.set == this)
            {
                throw new Exception("the item insert has already added");
            }
            else if (item.set != this)
            {
                throw new Exception("the item insert has already added to another set");
            }
        }
        
        private void InsertAtIndexNoCheck(int insertIndex, T item)
        {
            this.CheckCapacity(m_Count + 1);

            for (int i = m_Count; i > insertIndex; --i)
            {
                m_Buffer[i] = m_Buffer[i - 1];// move
                m_Buffer[i].KeyIndex = i;// index changed
            }

            m_Buffer[insertIndex] = item;
            item.set = this;
            item.KeyIndex = insertIndex;// index set
            ++m_Count;
                
            ++m_Version;
        }

        public bool Contains(T item)
        {
            return item.set == this;// && item == m_Buffer[item.index];
        }

        /// <summary>
        /// Remove will break index order
        /// </summary>
        public void Remove(T item)
        {
            if (item.set == this && item == m_Buffer[item.KeyIndex])
            {
                int index = item.KeyIndex;
                int last = m_Count - 1;
                
                if (index == last)
                {
                    m_Buffer[index] = null;
                    --m_Count;
                }
                else
                {
                    m_Buffer[index] = m_Buffer[last];
                    m_Buffer[index].KeyIndex = index;// index changed
                    m_Buffer[last] = null;
                    --m_Count;
                }

                ++m_Version;
                item.set = null;
            }
            else// if (throwException)
            {
                throw new Exception("the item is not belong to set, can not remove it");
            }
        }

        public T PopLast()
        {
            if (m_Count == 0)
            {
                throw new Exception("pop, but count 0");
            }

            T item = m_Buffer[--m_Count];
            item.set = null;
            ++m_Version;
            return item;
        }

        /* 此接口不安全
        public void RemoveAt(int index)
        {
            if (index >= 0 && index < m_Count)
            {
                this.Remove(m_Buffer[index]);
            }
        }
        */
    }

    public partial class KeySet<T>
    {
        public Enumerator GetEnumerator() => new Enumerator(this);
        
        public struct Enumerator
        {
            private KeySet<T> _this;
            private int _index;
            private int _version;

            internal Enumerator(KeySet<T> keySet)
            {
                _this = keySet;
                _index = -1;
                _version = keySet.m_Version;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (_version != _this.m_Version)
                {
                    throw new Exception("can not modify set in foreach");
                }

                if (++_index < _this.m_Count)
                {
                    return true;
                }

                return false;
            }

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _this.m_Buffer[_index];
            }
        }
    }
}