using System;
using System.Runtime.CompilerServices;

namespace Core.Collections
{
    public abstract class KeyDictionaryKey<TValue> : KeySetItem
    {
        internal TValue value;
    }

    public struct KeyDictionaryKeyPair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
    }

    public partial class KeyDictionary<TKey, TValue> : IDisposable where TKey : KeyDictionaryKey<TValue>
    {
        private KeySet<TKey> m_KeySet;
        
        ~KeyDictionary()
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
            if (m_KeySet.Count > 0)
            {
                foreach (var key in m_KeySet)
                {
                    key.value = default;
                    key.set = null;
                }
                // 一次循环同时清除数据与key，keyset内再次循环
                m_KeySet.ClearForDictionary();
            }
        }

        public void Add(TKey key, TValue value)
        {
            m_KeySet.Add(key);
            key.value = value;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            if (m_KeySet.Contains(key))
            {
                return false;
            }
            
            this.Add(key, value);
            return true;
        }

        public TValue this[TKey key]
        {
            get
            {
                if (m_KeySet.Contains(key))
                {
                    return key.value;
                }
                
                throw new Exception("can not find key");
            }
            set
            {
                if (m_KeySet.Contains(key))
                {
                    key.value = value;
                }
                else
                {
                    this.Add(key, value);
                }
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (m_KeySet.Contains(key))
            {
                value = key.value;
                return true;
            }

            value = default;
            return false;
        }

        public TValue GetValueOrDefault(TKey key)
        {
            if (m_KeySet.Contains(key))
            {
                return key.value;
            }
            return default;
        }

        public void RemoveKey(TKey key)
        {
            m_KeySet.Remove(key);
        }
    }
    
    public partial class KeyDictionary<TKey, TValue>
    {
        public Enumerator GetEnumerator() => new Enumerator(this);
        
        public struct Enumerator
        {
            private KeyDictionary<TKey, TValue> _this;
            private int _index;
            private int _version;

            internal Enumerator(KeyDictionary<TKey, TValue> dictionary)
            {
                _this = dictionary;
                _index = -1;
                _version = _this.m_KeySet.Version;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (_version != _this.m_KeySet.Version)
                {
                    throw new Exception("can not modify dictionary in foreach");
                }

                if (++_index < _this.m_KeySet.Count)
                {
                    return true;
                }

                return false;
            }

            public KeyDictionaryKeyPair<TKey, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    TKey key = _this.m_KeySet.Buffer[_index];
                    return new KeyDictionaryKeyPair<TKey, TValue>()
                    {
                        Key = key,
                        Value = key.value,
                    };
                }
            }
        }
    }
}