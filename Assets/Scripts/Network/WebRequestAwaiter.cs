using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequestAwaiter : INotifyCompletion
{
    private Action _continuation = default;

    private readonly UnityWebRequestAsyncOperation _asyncOperation = default;

    public bool IsCompleted => _asyncOperation.isDone;

    public WebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp)
    {
        _asyncOperation = asyncOp;
        _asyncOperation.completed += OnRequestCompleted;
    }

    public void GetResult() { }

    public void OnCompleted(Action continuation)
    {
        _continuation = continuation;
    }

    private void OnRequestCompleted(AsyncOperation obj)
    {
        _continuation();
    }
}

public static class WebRequestExtension
{
    public static WebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp) => new(asyncOp);
}
