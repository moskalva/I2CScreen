using System;
using System.Collections.Generic;
using System.Linq;

public class DisposableChain : IDisposable
{
    private readonly Stack<IDisposable> dependencies;

    public DisposableChain(params IDisposable[] dependencies)
    {
        this.dependencies = new Stack<IDisposable>(dependencies ?? throw new ArgumentNullException(nameof(dependencies)));
    }

    public T Add<T>(T dependency) where T : IDisposable
    {
        if (isDisposed) throw new InvalidOperationException("Instance was disposed already");

        this.dependencies.Push(dependency);
        return dependency;
    }

    private bool isDisposed = false;
    public void Dispose()
    {
        if (isDisposed) return;

        foreach (var entry in this.dependencies)
        {
            try
            {
                if (entry != null)
                    entry.Dispose();
            }
            catch (Exception) { }
        }
        isDisposed = true;
    }
}