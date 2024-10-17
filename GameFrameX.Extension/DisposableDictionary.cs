﻿namespace GameFrameX.Extension;

/// <summary>
/// 值可被Dispose的字典类型
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class DisposableDictionary<TKey, TValue> : NullableDictionary<TKey, TValue>, IDisposable where TValue : IDisposable
{
    private bool isDisposed;

    /// <summary>
    /// 终结器
    /// </summary>
    ~DisposableDictionary()
    {
        Dispose(false);
    }

    /// <summary>
    ///
    /// </summary>
    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        Dispose(true);
        isDisposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 
    /// </summary>
    public DisposableDictionary()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fallbackValue"></param>
    public DisposableDictionary(TValue fallbackValue) : base()
    {
        FallbackValue = fallbackValue;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="capacity"></param>
    public DisposableDictionary(int capacity) : base(capacity)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dictionary"></param>
    public DisposableDictionary(IDictionary<NullObject<TKey>, TValue> dictionary) : base(dictionary)
    {
    }

    /// <summary>
    /// 释放
    /// </summary>
    /// <param name="disposing"></param>
    public void Dispose(bool disposing)
    {
        foreach (var s in Values.Where(v => v != null))
        {
            s.Dispose();
        }
    }
}