using System;

namespace VacuumBreather.Mvvm.Core.Extensions;

/// <summary>Provides extension methods for the <see cref="IAsyncOperation" /> type.</summary>
public static class AsyncOperationExtensions
{
    public static IAsyncOperation Assign(this IAsyncOperation that, out IAsyncOperation asyncOperation)
    {
        asyncOperation = that;

        return that;
    }

    public static IAsyncOperation CancelWhenDeactivating(this IAsyncOperation that, IDeactivate deactivate)
    {
        var weakReference = new WeakReference(that);

        deactivate.Deactivating += async (_, _, _) =>
        {
            if (weakReference.Target is IAsyncOperation operation)
            {
                operation.Cancel();

                await operation;
            }
        };

        return that;
    }

    public static IAsyncOperation CancelWhenActivating(this IAsyncOperation that, IActivate deactivate)
    {
        var weakReference = new WeakReference(that);

        deactivate.Activating += async (_, _, _) =>
        {
            if (weakReference.Target is IAsyncOperation operation)
            {
                operation.Cancel();

                await operation;
            }
        };

        return that;
    }

    public static IAsyncOperation CancelWhenClosing(this IAsyncOperation that, IDeactivate deactivate)
    {
        var weakReference = new WeakReference(that);

        deactivate.Deactivating += async (_, args, _) =>
        {
            if (args.WillClose && weakReference.Target is IAsyncOperation operation)
            {
                operation.Cancel();

                await operation;
            }
        };

        return that;
    }
}
