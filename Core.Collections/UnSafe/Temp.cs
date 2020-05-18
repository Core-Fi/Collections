using System;

namespace Core.Collections.UnSafe
{
    public unsafe struct StackList<T> where T : unmanaged
    {
        private T* m_Buffer;
        private int m_Capacity;
        private int m_Count;
        
        public StackList(T* array, int capacity)
        {
            m_Buffer = array;
            m_Capacity = capacity;
            m_Count = 0;
        }
    }
}