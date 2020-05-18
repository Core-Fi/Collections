using System;
using System.Runtime.CompilerServices;

namespace Core.Collections.UnSafe
{
    public interface IKeySetItem
    {
        KeySetItem KeySetItem { get; set; }
    }

    public struct KeySetItem
    {
        private int m_Index;
        internal object set;

        public int index
        {
            get => m_Index;
            internal set => m_Index = value;
        }
    }

    public partial class KeySet<T> : IDisposable where T : class, IKeySetItem
    {
        private T[] m_Buffer;
        private int m_Count;

        public int Count => m_Count;

        public KeySet() : this(16)
        {
        }

        public KeySet(int capacity)
        {
            if (capacity < 0)
            {
                throw new Exception("capacity must > 0");
            }

            m_Buffer = new T[capacity];
            m_Count = 0;
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
                
                GC.SuppressFinalize(this);
            }
        }

        public void Add(T item)
        {
            if (item == null)
            {
                throw new NullReferenceException();
            }
            
            if (item.KeySetItem.set == null)
            {
                item.KeySetItem = new KeySetItem()
                {
                    set = this,
                    index = m_Count,
                };

                m_Buffer[m_Count++] = item;
            }
            else if (item.KeySetItem.set == this)
            {
                throw new Exception("the item has already added");
            }
            else if (item.KeySetItem.set == this)
            {
                throw new Exception("the item has already added to another set");
            }
        }
        public void Add(ref T item)
        {
            if (item.KeySetItem.set == null)
            {
                item.KeySetItem = new KeySetItem()
                {
                    set = this,
                    index = m_Count,
                };

                m_Buffer[m_Count++] = item;
            }
            else if (item.KeySetItem.set == this)
            {
                throw new Exception("the item has already added");
            }
            else if (item.KeySetItem.set == this)
            {
                throw new Exception("the item has already added to another set");
            }
        }

        public void Remove(T item)
        {
            this.Remove(item, false);
        }

        public void Remove(T item, bool throwException)
        {
            if (item.KeySetItem.set == this)
            {
                int index = item.KeySetItem.index;
                int last = m_Count - 1;
                
                if (index == last)
                {
                    m_Buffer[index] = default;
                    --m_Count;
                }
                else
                {
                    m_Buffer[index] = m_Buffer[last];
                    m_Buffer[last] = default;
                    --m_Count;
                }
            }
            else if (throwException)
            {
                throw new Exception("the item is not belong to set, can not remove it");
            }
        }

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < m_Count)
            {
                this.Remove(m_Buffer[index], false);
            }
        }
    }

    public partial class KeySet<T>
    {
        public Enumerator GetEnumerator() => new Enumerator(this);
        
        public struct Enumerator
        {
            private KeySet<T> _this;
            private int _index;

            internal Enumerator(KeySet<T> keySet)
            {
                _this = keySet;
                _index = -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
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