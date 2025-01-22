using System;
using System.Threading;
using System.Threading.Tasks;

public static class MainThreadDispatcher
{
    private static SynchronizationContext _mainThreadContext;

    public static void SetMainThreadContext()
    {
        var current = SynchronizationContext.Current;
        _mainThreadContext = current ?? throw new InvalidOperationException();
    }

    // メインスレッドでアクションを実行
    public static void Post(Action action)
    {
        if (_mainThreadContext == null)
            throw new InvalidOperationException();

        _mainThreadContext.Post(_ => action(), null);
    }

    public static Task<TResult> RunAsync<TResult>(Func<Task<TResult>> func)
    {
        var tcs = new TaskCompletionSource<TResult>();
        Post(async () =>
        {
            try
            {
                var res = await func();
                tcs.SetResult(res);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }
}

