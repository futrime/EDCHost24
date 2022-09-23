using System;
using System.Collections.Generic;

namespace EdcHost;

public class MyQueue<T>
{
    private int MAX_LONGTH;

    private List<T> mItem;

    public MyQueue(int _MaxLongth)
    {
        if (_MaxLongth < 1)
        {
            throw new Exception("MaxLongth is expected to be larger than 0");
        }
        MAX_LONGTH = _MaxLongth;
        mItem = new List<T>();
    }

    public void Enqueue(T _item)
    {
        if (mItem.Count < MAX_LONGTH)
        {
            mItem.Add(_item);
        }
        else if (mItem.Count == MAX_LONGTH)
        {
            mItem.RemoveAt(0);
            mItem.Add(_item);
        }
    }

    public T Item(int _index)
    {
        if (_index < mItem.Count && _index >= 0)
        {
            return mItem[_index];
        }
        else if (_index <= -1 && _index >= -mItem.Count)
        {
            return mItem[mItem.Count + _index];
        }
        else
        {
            throw new Exception("PosQueue: index is out of range");
        }
    }

    public int Count()
    {
        return mItem.Count;
    }

    public void Clear()
    {
        mItem.Clear();
    }
}
