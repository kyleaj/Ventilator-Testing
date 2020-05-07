using System;
using System.Collections.Generic;
using System.Text;

namespace VentilatorTestConsole
{
    public class ShiftList<T>
    {
        private const int DEFAULT_LENGTH = 100;

        private int CurrentIndex;
        private int Size;
        //private int Capacity;
        private T[] Items;

        public ShiftList()
        {
            CurrentIndex = 0;
            Size = 0;
            Items = new T[DEFAULT_LENGTH];
        }

        public ShiftList(T defaultValue)
        {
            CurrentIndex = 0;
            Size = 0;
            Items = new T[DEFAULT_LENGTH];
            for (int i = 0; i < Items.Length; i++)
            {
                Items[i] = defaultValue;
            }
        }

        public ShiftList(int Capacity, T defaultValue)
        {
            CurrentIndex = 0;
            Size = 0;
            Items = new T[Capacity];
            for (int i = 0; i < Items.Length; i++)
            {
                Items[i] = defaultValue;
            }
        }

        public void Add(T item)
        {
            Items[(CurrentIndex + Size) % Items.Length] = item;
            if (Size == Items.Length)
            {
                CurrentIndex++;
            } else
            {
                Size++;
            }
        }

        public void Set(int index, T item)
        {
            // Alternatively, take the modulus with Items.Length?
            if (index >= Items.Length)
            {
                throw new IndexOutOfRangeException();
            }
            Items[index] = item;
        }

        public T Get(int index)
        {
            // Alternatively, take the modulus with Items.Length?
            if (index >= Items.Length)
            {
                throw new IndexOutOfRangeException();
            }
            return Items[(index + CurrentIndex) % Items.Length];
        }
    }
}
