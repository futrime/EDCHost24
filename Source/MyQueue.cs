using System;
using System.Collections.Generic;

namespace EdcHost;

public class MyQueue<T>
{
    // 计算车的位移，因而我们需要保存最近几帧中车的位置(Dot)，所以用一个Queue来存储。
    // MAX_LENGTH即位置的最大存储数量，暂定为10个
    private int MAX_LENGTH;

    //保存车的位置的列表
    private List<T> mItem;

    public MyQueue(int _MaxLength)
    {
        if (_MaxLength < 1)
        {
            throw new Exception("MaxLength is expected to be larger than 0");
        }
        MAX_LENGTH = _MaxLength;
        mItem = new List<T>();
    }

    public void Enqueue(T _item)
    {
        if (mItem.Count < MAX_LENGTH)
        {
            mItem.Add(_item);
        }
        else if (mItem.Count == MAX_LENGTH)
        {
            mItem.RemoveAt(0);
            mItem.Add(_item);
        }
    }
    // getter()  _index为-1即最后一个 -2为倒数第二个
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
