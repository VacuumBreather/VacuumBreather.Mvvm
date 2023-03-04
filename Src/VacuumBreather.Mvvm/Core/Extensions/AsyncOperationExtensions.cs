using System;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Provides extension methods for the <see cref="IAsyncOperation" /> type.</summary>
[PublicAPI]
public static class AsyncOperationExtensions
{
    /// <summary>
    ///     Assigns the <see cref="IAsyncOperation" /> to an out variable for external use.
    /// </summary>
    /// <param name="operation">The <see cref="IAsyncOperation" />.</param>
    /// <param name="asyncOperation">The out variable to assign the <see cref="IAsyncOperation" /> to.</param>
    /// <returns>The same instance of the <see cref="IAsyncOperation" /> for chaining.</returns>
    public static IAsyncOperation Assign(this IAsyncOperation operation, out IAsyncOperation asyncOperation)
    {
        asyncOperation = operation;

        return operation;
    }

    /// <summary>
    ///     Causes the <see cref="IActivate.Activating" /> event to cancel this operation.
    /// </summary>
    /// <param name="operation">The <see cref="IAsyncOperation" />.</param>
    /// <param name="activate">The <see cref="IActivate" /> that triggers the cancellation.</param>
    /// <returns>The same instance of the <see cref="IAsyncOperation" /> for chaining.</returns>
    public static IAsyncOperation CancelWhenActivating(this IAsyncOperation operation, IActivate activate)
    {
        var weakReference = new WeakReference(operation);

        activate.Activating += async (_, _, _) =>
        {
            if (weakReference.Target is IAsyncOperation referencedOperation)
            {
                referencedOperation.Cancel();

                await referencedOperation;
            }
        };

        return operation;
    }

    /// <summary>
    ///     Causes the <see cref="IDeactivate.Deactivating" /> event to cancel this operation when the
    ///     <see cref="DeactivatingEventArgs.WillClose" /> flag is set.
    /// </summary>
    /// <param name="operation">The <see cref="IAsyncOperation" />.</param>
    /// <param name="deactivate">The <see cref="IDeactivate" /> that triggers the cancellation on closing.</param>
    /// <returns>The same instance of the <see cref="IAsyncOperation" /> for chaining.</returns>
    public static IAsyncOperation CancelWhenClosing(this IAsyncOperation operation, IDeactivate deactivate)
    {
        var weakReference = new WeakReference(operation);

        deactivate.Deactivating += async (_, args, _) =>
        {
            if (args.WillClose && weakReference.Target is IAsyncOperation referencedOperation)
            {
                referencedOperation.Cancel();

                await referencedOperation;
            }
        };

        return operation;
    }

    /// <summary>
    ///     Causes the <see cref="IDeactivate.Deactivating" /> event to cancel this operation.
    /// </summary>
    /// <param name="operation">The <see cref="IAsyncOperation" />.</param>
    /// <param name="deactivate">The <see cref="IDeactivate" /> that triggers the cancellation.</param>
    /// <returns>The same instance of the <see cref="IAsyncOperation" /> for chaining.</returns>
    public static IAsyncOperation CancelWhenDeactivating(this IAsyncOperation operation, IDeactivate deactivate)
    {
        var weakReference = new WeakReference(operation);

        deactivate.Deactivating += async (_, _, _) =>
        {
            if (weakReference.Target is IAsyncOperation referencedOperation)
            {
                referencedOperation.Cancel();

                await referencedOperation;
            }
        };

        return operation;
    }
}