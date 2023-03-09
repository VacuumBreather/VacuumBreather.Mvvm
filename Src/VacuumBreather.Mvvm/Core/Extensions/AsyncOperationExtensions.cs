using System;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Provides extension methods for the <see cref="IAsyncOperation"/> type.</summary>
[PublicAPI]
public static class AsyncOperationExtensions
{
    /// <summary>Assigns the <see cref="IAsyncOperation"/> to an out variable for external use.</summary>
    /// <typeparam name="TAsyncOperation">The type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation.</param>
    /// <param name="asyncOperation">The out variable to assign the <see cref="IAsyncOperation"/> to.</param>
    /// <returns>The same instance of the <see cref="IAsyncOperation"/> for chaining.</returns>
    public static IAsyncOperation Assign<TAsyncOperation>(this TAsyncOperation operation,
                                                          out IAsyncOperation asyncOperation)
        where TAsyncOperation : class, IAsyncOperation
    {
        asyncOperation = operation;

        return operation;
    }

    /// <summary>Causes the <see cref="IActivate.Activating"/> event to cancel this operation.</summary>
    /// <typeparam name="TAsyncOperation">The type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation.</param>
    /// <param name="activate">The <see cref="IActivate"/> that triggers the cancellation.</param>
    /// <returns>The same instance of the <see cref="IAsyncOperation"/> for chaining.</returns>
    public static IAsyncOperation CancelWhenActivating<TAsyncOperation>(this TAsyncOperation operation,
                                                                        IActivate activate)
        where TAsyncOperation : class, IAsyncOperation
    {
        var weakReference = new WeakReference(operation);

        activate.Activating += async (_, _, _) =>
        {
            if (weakReference.Target is IAsyncOperation referencedOperation)
            {
                referencedOperation.Cancel();

                try
                {
                    await referencedOperation;
                }
                catch (OperationCanceledException)
                {
                    // Ignore this expected exception.
                }
            }
        };

        return operation;
    }

    /// <summary>
    ///     Causes the <see cref="IDeactivate.Deactivating"/> event to cancel this operation when the
    ///     <see cref="DeactivatingEventArgs.WillClose"/> flag is set.
    /// </summary>
    /// <typeparam name="TAsyncOperation">The type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation.</param>
    /// <param name="deactivate">The <see cref="IDeactivate"/> that triggers the cancellation on closing.</param>
    /// <returns>The same instance of the <see cref="IAsyncOperation"/> for chaining.</returns>
    public static IAsyncOperation CancelWhenClosing<TAsyncOperation>(this TAsyncOperation operation,
                                                                     IDeactivate deactivate)
        where TAsyncOperation : class, IAsyncOperation
    {
        var weakReference = new WeakReference(operation);

        deactivate.Deactivating += async (_, args, _) =>
        {
            if (args.WillClose && weakReference.Target is IAsyncOperation referencedOperation)
            {
                referencedOperation.Cancel();

                try
                {
                    await referencedOperation;
                }
                catch (OperationCanceledException)
                {
                    // Ignore this expected exception.
                }
            }
        };

        return operation;
    }

    /// <summary>Causes the <see cref="IDeactivate.Deactivating"/> event to cancel this operation.</summary>
    /// <typeparam name="TAsyncOperation">The type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation.</param>
    /// <param name="deactivate">The <see cref="IDeactivate"/> that triggers the cancellation.</param>
    /// <returns>The same instance of the <see cref="IAsyncOperation"/> for chaining.</returns>
    public static IAsyncOperation CancelWhenDeactivating<TAsyncOperation>(
        this TAsyncOperation operation,
        IDeactivate deactivate)
        where TAsyncOperation : class, IAsyncOperation
    {
        var weakReference = new WeakReference(operation);

        deactivate.Deactivating += async (_, _, _) =>
        {
            if (weakReference.Target is IAsyncOperation referencedOperation)
            {
                referencedOperation.Cancel();

                try
                {
                    await referencedOperation;
                }
                catch (OperationCanceledException)
                {
                    // Ignore this expected exception.
                }
            }
        };

        return operation;
    }
}