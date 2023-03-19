using System;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Provides extension methods for the <see cref="ICancellable"/> type.</summary>
[PublicAPI]
public static class CancellableExtensions
{
    /// <summary>Causes the <see cref="IActivate.Activating"/> event to cancel this <see cref="ICancellable"/>.</summary>
    /// <typeparam name="TCancellable">The type of <see cref="ICancellable"/>.</typeparam>
    /// <param name="cancellable">The instance that should be cancelled.</param>
    /// <param name="activate">The <see cref="IActivate"/> that triggers the cancellation.</param>
    /// <returns>The same instance of the <see cref="ICancellable"/> for chaining.</returns>
    public static TCancellable CancelWhenActivating<TCancellable>(this TCancellable cancellable, IActivate activate)
        where TCancellable : class, ICancellable
    {
        var weakReference = new WeakReference(cancellable);

        activate.Activating += async (_, _, _) =>
        {
            if (weakReference.Target is ICancellable referencedOperation)
            {
                referencedOperation.Cancel();

                if (weakReference.Target is IAwaitable awaitable)
                {
                    try
                    {
                        await awaitable;
                    }
                    catch (OperationCanceledException)
                    {
                        // Ignore this expected exception.
                    }
                }
            }
        };

        return cancellable;
    }

    /// <summary>
    ///     Causes the <see cref="IDeactivate.Deactivating"/> event to cancel this <see cref="ICancellable"/> when the
    ///     <see cref="DeactivatingEventArgs.WillClose"/> flag is set.
    /// </summary>
    /// <typeparam name="TCancellable">The type of <see cref="ICancellable"/>.</typeparam>
    /// <param name="cancellable">The instance that should be cancelled.</param>
    /// <param name="deactivate">The <see cref="IDeactivate"/> that triggers the cancellation on closing.</param>
    /// <returns>The same instance of the <see cref="ICancellable"/> for chaining.</returns>
    public static TCancellable CancelWhenClosing<TCancellable>(this TCancellable cancellable, IDeactivate deactivate)
        where TCancellable : class, ICancellable
    {
        var weakReference = new WeakReference(cancellable);

        deactivate.Deactivating += async (_, args, _) =>
        {
            if (args.WillClose && weakReference.Target is ICancellable referencedOperation)
            {
                referencedOperation.Cancel();

                if (weakReference.Target is IAwaitable awaitable)
                {
                    try
                    {
                        await awaitable;
                    }
                    catch (OperationCanceledException)
                    {
                        // Ignore this expected exception.
                    }
                }
            }
        };

        return cancellable;
    }

    /// <summary>Causes the <see cref="IDeactivate.Deactivating"/> event to cancel this <see cref="ICancellable"/>.</summary>
    /// <typeparam name="TCancellable">The type of <see cref="ICancellable"/>.</typeparam>
    /// <param name="cancellable">The instance that should be cancelled.</param>
    /// <param name="deactivate">The <see cref="IDeactivate"/> that triggers the cancellation.</param>
    /// <returns>The same instance of the <see cref="ICancellable"/> for chaining.</returns>
    public static TCancellable CancelWhenDeactivating<TCancellable>(this TCancellable cancellable,
                                                                    IDeactivate deactivate)
        where TCancellable : class, ICancellable
    {
        var weakReference = new WeakReference(cancellable);

        deactivate.Deactivating += async (_, _, _) =>
        {
            if (weakReference.Target is ICancellable referencedOperation)
            {
                referencedOperation.Cancel();

                if (weakReference.Target is IAwaitable awaitable)
                {
                    try
                    {
                        await awaitable;
                    }
                    catch (OperationCanceledException)
                    {
                        // Ignore this expected exception.
                    }
                }
            }
        };

        return cancellable;
    }
}