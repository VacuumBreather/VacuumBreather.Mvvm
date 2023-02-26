﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Diagnostics;
using Microsoft.VisualStudio.Threading;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Enables easy marshalling of code to the UI thread.</summary>
public static class ThreadHelper
{
    private static readonly Dispatcher? Dispatcher = Application.Current?.Dispatcher;
    private static readonly CancellationTokenSource DisposeCancellationTokenSource = new();
    private static readonly CancellationToken DisposalToken = DisposeCancellationTokenSource.Token;

    private static readonly JoinableTaskContext? JoinableTaskContext;
    private static readonly JoinableTaskCollection? JoinableTaskCollection;
    private static readonly JoinableTaskFactory? JoinableTaskFactory;

    /// <summary>
    ///     Initializes static members of the <see cref="ThreadHelper" /> class.
    /// </summary>
    static ThreadHelper()
    {
        JoinableTaskContext = Dispatcher is null
            ? null
            : new JoinableTaskContext(Dispatcher.Thread, new DispatcherSynchronizationContext(Dispatcher));

        JoinableTaskCollection = JoinableTaskContext?.CreateCollection();
        JoinableTaskFactory = JoinableTaskContext?.CreateFactory(JoinableTaskCollection!);

        if (Application.Current is { } application)
        {
            application.Exit += OnCurrentExit;
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the called is on the UI thread.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the called is on the UI thread; otherwise, <see langword="false" />.
    /// </value>
    public static bool IsOnUIThread => JoinableTaskContext?.IsOnMainThread ?? true;

    /// <summary>
    ///     Cleans up all unfinished async work. Consecutive calls will have no effect.
    /// </summary>
    /// <exception cref="AggregateException">Any unfinished tasks threw an exception.</exception>
    public static void CleanUp()
    {
        if (DisposalToken.IsCancellationRequested)
        {
            return;
        }

        DisposeCancellationTokenSource.Cancel();

        if (Application.Current is { } application)
        {
            application.Exit -= OnCurrentExit;
        }

        try
        {
            if (JoinableTaskCollection is not null)
            {
                using var cts = new CancellationTokenSource();
                using var context = new JoinableTaskContext(Dispatcher!.Thread);

                var token = cts.Token;
                var taskFactory = new JoinableTaskFactory(context);

                cts.CancelAfter(3000);
                taskFactory.Run(() => JoinableTaskCollection.JoinTillEmptyAsync(token));
            }
        }
        catch (OperationCanceledException)
        {
            // This exception is expected because we signaled the cancellation token
        }
        catch (AggregateException ex)
        {
            // Ignore AggregateException containing only OperationCanceledException
            ex.Handle(inner => inner is OperationCanceledException);
        }
        finally
        {
            JoinableTaskContext?.Dispose();
            DisposeCancellationTokenSource.Dispose();
        }
    }

    /// <summary>
    ///     Executes the specified operation on a background thread and runs it to completion while synchronously
    ///     blocking the calling thread.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnBackgroundThread(this Action operation,
                                             JoinableTaskCreationOptions creationOptions =
                                                 JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        if (JoinableTaskFactory is null)
        {
            operation();
        }
        else
        {
            JoinableTaskFactory.Run(async () =>
                                    {
                                        // Switch to background thread.
                                        await TaskScheduler.Default;

                                        operation();
                                    },
                                    creationOptions);
        }
    }

    /// <summary>
    ///     Executes the specified asynchronous operation on a background thread and runs it to completion while synchronously
    ///     blocking the calling thread.
    /// </summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnBackgroundThread(this Func<ValueTask> operation,
                                             JoinableTaskCreationOptions creationOptions =
                                                 JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        if (JoinableTaskFactory is null)
        {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            Task.Run(async () => await operation().ConfigureAwait(true)).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }
        else
        {
            JoinableTaskFactory.Run(async () =>
                                    {
                                        // Switch to background thread.
                                        await TaskScheduler.Default;

                                        await operation().ConfigureAwait(true);
                                    },
                                    creationOptions);
        }
    }

    /// <summary>
    ///     Executes the specified asynchronous operation on a background thread and runs it to completion while synchronously
    ///     blocking the calling thread.
    /// </summary>
    /// <typeparam name="T">The result type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>The result of the asynchronous operation.</returns>
    public static T RunOnBackgroundThread<T>(this Func<ValueTask<T>> operation,
                                             JoinableTaskCreationOptions creationOptions =
                                                 JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        if (JoinableTaskFactory is null)
        {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            return Task.Run(async () => await operation().ConfigureAwait(true)).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }

        return JoinableTaskFactory.Run(async () =>
                                       {
                                           // Switch to background thread.
                                           await TaskScheduler.Default;

                                           return await operation().ConfigureAwait(true);
                                       },
                                       creationOptions);
    }

    /// <summary>
    ///     Executes the specified operation on a background thread and runs it to completion while synchronously
    ///     blocking the calling thread.
    /// </summary>
    /// <typeparam name="T">The result type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>The result of the operation.</returns>
    public static T RunOnBackgroundThread<T>(this Func<T> operation,
                                             JoinableTaskCreationOptions creationOptions =
                                                 JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        if (JoinableTaskFactory is null)
        {
            return operation();
        }

        return JoinableTaskFactory.Run(async () =>
                                       {
                                           // Switch to background thread.
                                           await TaskScheduler.Default;

                                           return operation();
                                       },
                                       creationOptions);
    }

    /// <summary>
    ///     Executes the specified operation asynchronously on a background thread and ignores the result.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnBackgroundThreadAndForget(this Action operation,
                                                      JoinableTaskCreationOptions creationOptions =
                                                          JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        operation.RunOnBackgroundThreadAsync(creationOptions).Forget();
    }

    /// <summary>
    ///     Executes the specified asynchronous operation on a background thread and ignores the result.
    /// </summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnBackgroundThreadAndForget(this Func<ValueTask> operation,
                                                      JoinableTaskCreationOptions creationOptions =
                                                          JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        operation.RunOnBackgroundThreadAsync(creationOptions).Forget();
    }

    /// <summary>
    ///     Executes the specified asynchronous operation on a background thread and ignores the result.
    /// </summary>
    /// <typeparam name="T">The result type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnBackgroundThreadAndForget<T>(this Func<ValueTask<T>> operation,
                                                         JoinableTaskCreationOptions creationOptions =
                                                             JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        operation.RunOnBackgroundThreadAsync(creationOptions).Forget();
    }

    /// <summary>
    ///     Executes the specified operation asynchronously on a background thread and ignores the result.
    /// </summary>
    /// <typeparam name="T">The result type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnBackgroundThreadAndForget<T>(this Func<T> operation,
                                                         JoinableTaskCreationOptions creationOptions =
                                                             JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        operation.RunOnBackgroundThreadAsync(creationOptions).Forget();
    }

    /// <summary>
    ///     Executes the specified operation asynchronously on a background thread.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async ValueTask RunOnBackgroundThreadAsync(this Action operation,
                                                             JoinableTaskCreationOptions creationOptions =
                                                                 JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        if (JoinableTaskFactory is null)
        {
            await Task.Run(operation).ConfigureAwait(true);
        }
        else
        {
            await JoinableTaskFactory.RunAsync(async () =>
                                               {
                                                   // Switch to background thread.
                                                   await TaskScheduler.Default;

                                                   operation();
                                               },
                                               creationOptions);
        }
    }

    /// <summary>
    ///     Executes the specified asynchronous operation on a background thread.
    /// </summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async ValueTask RunOnBackgroundThreadAsync(this Func<ValueTask> operation,
                                                             JoinableTaskCreationOptions creationOptions =
                                                                 JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        if (JoinableTaskFactory is null)
        {
            await Task.Run(async () => await operation().ConfigureAwait(true)).ConfigureAwait(true);
        }
        else
        {
            await JoinableTaskFactory.RunAsync(async () =>
                                               {
                                                   // Switch to background thread.
                                                   await TaskScheduler.Default;

                                                   await operation().ConfigureAwait(true);
                                               },
                                               creationOptions);
        }
    }

    /// <summary>
    ///     Executes the specified asynchronous operation on a background thread.
    /// </summary>
    /// <typeparam name="T">The result type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async ValueTask<T> RunOnBackgroundThreadAsync<T>(this Func<ValueTask<T>> operation,
                                                                   JoinableTaskCreationOptions creationOptions =
                                                                       JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        if (JoinableTaskFactory is null)
        {
            return await Task.Run(async () => await operation().ConfigureAwait(true)).ConfigureAwait(true);
        }

        return await JoinableTaskFactory.RunAsync(async () =>
                                                  {
                                                      // Switch to background thread.
                                                      await TaskScheduler.Default;

                                                      return await operation().ConfigureAwait(true);
                                                  },
                                                  creationOptions);
    }

    /// <summary>
    ///     Executes the specified operation asynchronously on a background thread.
    /// </summary>
    /// <typeparam name="T">The result type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The task result contains the result of the operation.
    /// </returns>
    public static async ValueTask<T> RunOnBackgroundThreadAsync<T>(this Func<T> operation,
                                                                   JoinableTaskCreationOptions creationOptions =
                                                                       JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        if (JoinableTaskFactory is null)
        {
            return await Task.Run(operation).ConfigureAwait(true);
        }

        return await JoinableTaskFactory.RunAsync(async () =>
                                                  {
                                                      // Switch to background thread.
                                                      await TaskScheduler.Default;

                                                      return operation();
                                                  },
                                                  creationOptions);
    }

    /// <summary>
    ///     Executes the specified operation on the UI thread and runs it to completion while synchronously
    ///     blocking the calling thread.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnUIThread(this Action operation,
                                     JoinableTaskCreationOptions creationOptions = JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        if (JoinableTaskFactory is null)
        {
            operation();
        }
        else
        {
            JoinableTaskFactory.Run(async () =>
                                    {
                                        // Switch to UI thread.
                                        await JoinableTaskFactory.SwitchToMainThreadAsync();

                                        operation();
                                    },
                                    creationOptions);
        }
    }

    /// <summary>
    ///     Executes the specified asynchronous operation on the UI thread and runs it to completion while synchronously
    ///     blocking the calling thread.
    /// </summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnUIThread(this Func<ValueTask> operation,
                                     JoinableTaskCreationOptions creationOptions = JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        if (JoinableTaskFactory is null)
        {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            Task.Run(async () => await operation().ConfigureAwait(true)).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }
        else
        {
            JoinableTaskFactory.Run(async () =>
                                    {
                                        // Switch to UI thread.
                                        await JoinableTaskFactory.SwitchToMainThreadAsync();
                                        await operation().ConfigureAwait(true);
                                    },
                                    creationOptions);
        }
    }

    /// <summary>
    ///     Executes the specified asynchronous operation on the UI thread and runs it to completion while synchronously
    ///     blocking the calling thread.
    /// </summary>
    /// <typeparam name="T">The result type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>The result of the asynchronous operation.</returns>
    public static T RunOnUIThread<T>(this Func<ValueTask<T>> operation,
                                     JoinableTaskCreationOptions creationOptions = JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        if (JoinableTaskFactory is null)
        {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            return Task.Run(async () => await operation().ConfigureAwait(true)).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }

        return JoinableTaskFactory.Run(async () =>
                                       {
                                           // Switch to UI thread.
                                           await JoinableTaskFactory.SwitchToMainThreadAsync();

                                           return await operation().ConfigureAwait(true);
                                       },
                                       creationOptions);
    }

    /// <summary>
    ///     Executes the specified operation on the UI thread and runs it to completion while synchronously
    ///     blocking the calling thread.
    /// </summary>
    /// <typeparam name="T">The result type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>The result of the operation.</returns>
    public static T RunOnUIThread<T>(this Func<T> operation,
                                     JoinableTaskCreationOptions creationOptions = JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        if (JoinableTaskFactory is null)
        {
            return operation();
        }

        return JoinableTaskFactory.Run(async () =>
                                       {
                                           // Switch to UI thread.
                                           await JoinableTaskFactory.SwitchToMainThreadAsync();

                                           return operation();
                                       },
                                       creationOptions);
    }

    /// <summary>
    ///     Executes the specified operation asynchronously on the UI thread and ignores the result.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnUIThreadAndForget(this Action operation,
                                              JoinableTaskCreationOptions creationOptions =
                                                  JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        operation.RunOnUIThreadAsync(creationOptions).Forget();
    }

    /// <summary>
    ///     Executes the specified asynchronous operation on the UI thread and ignores the result.
    /// </summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnUIThreadAndForget(this Func<ValueTask> operation,
                                              JoinableTaskCreationOptions creationOptions =
                                                  JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        operation.RunOnUIThreadAsync(creationOptions).Forget();
    }

    /// <summary>
    ///     Executes the specified asynchronous operation on the UI thread and ignores the result.
    /// </summary>
    /// <typeparam name="T">The result type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnUIThreadAndForget<T>(this Func<ValueTask<T>> operation,
                                                 JoinableTaskCreationOptions creationOptions =
                                                     JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        operation.RunOnUIThreadAsync(creationOptions).Forget();
    }

    /// <summary>
    ///     Executes the specified operation asynchronously on the UI thread and ignores the result.
    /// </summary>
    /// <typeparam name="T">The result type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnUIThreadAndForget<T>(this Func<T> operation,
                                                 JoinableTaskCreationOptions creationOptions =
                                                     JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        operation.RunOnUIThreadAsync(creationOptions).Forget();
    }

    /// <summary>
    ///     Executes the specified operation asynchronously on the UI thread.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async ValueTask RunOnUIThreadAsync(this Action operation,
                                                     JoinableTaskCreationOptions creationOptions =
                                                         JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        if (JoinableTaskFactory is null)
        {
            operation();
        }
        else
        {
            await JoinableTaskFactory.RunAsync(async () =>
                                               {
                                                   // Switch to UI thread.
                                                   await JoinableTaskFactory.SwitchToMainThreadAsync();

                                                   operation();
                                               },
                                               creationOptions);
        }
    }

    /// <summary>
    ///     Executes the specified asynchronous operation on the UI thread.
    /// </summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async ValueTask RunOnUIThreadAsync(this Func<ValueTask> operation,
                                                     JoinableTaskCreationOptions creationOptions =
                                                         JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        if (JoinableTaskFactory is null)
        {
            await operation().ConfigureAwait(true);
        }
        else
        {
            await JoinableTaskFactory.RunAsync(async () =>
                                               {
                                                   // Switch to UI thread.
                                                   await JoinableTaskFactory.SwitchToMainThreadAsync();
                                                   await operation().ConfigureAwait(true);
                                               },
                                               creationOptions);
        }
    }

    /// <summary>
    ///     Executes the specified asynchronous operation on the UI thread.
    /// </summary>
    /// <typeparam name="T">The result type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The task result contains the result of the operation.
    /// </returns>
    public static async ValueTask<T> RunOnUIThreadAsync<T>(this Func<ValueTask<T>> operation,
                                                           JoinableTaskCreationOptions creationOptions =
                                                               JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        if (JoinableTaskFactory is null)
        {
            return await operation().ConfigureAwait(true);
        }

        return await JoinableTaskFactory.RunAsync(async () =>
                                                  {
                                                      // Switch to UI thread.
                                                      await JoinableTaskFactory.SwitchToMainThreadAsync();

                                                      return await operation().ConfigureAwait(true);
                                                  },
                                                  creationOptions);
    }

    /// <summary>
    ///     Executes the specified operation asynchronously on the UI thread.
    /// </summary>
    /// <typeparam name="T">The result type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions" /> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The task result contains the result of the operation.
    /// </returns>
    public static async ValueTask<T> RunOnUIThreadAsync<T>(this Func<T> operation,
                                                           JoinableTaskCreationOptions creationOptions =
                                                               JoinableTaskCreationOptions.None)
    {
        ThrowIfDisposed();

        if (JoinableTaskFactory is null)
        {
            return operation();
        }

        return await JoinableTaskFactory.RunAsync(async () =>
                                                  {
                                                      // Switch to UI thread.
                                                      await JoinableTaskFactory.SwitchToMainThreadAsync();

                                                      return operation();
                                                  },
                                                  creationOptions);
    }

    /// <summary>
    ///     Throws an <see cref="InvalidOperationException" /> if not called from the UI thread.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public static void ThrowIfNotOnUIThread(string message)
    {
        if (!JoinableTaskContext?.IsOnMainThread ?? false)
        {
            ThrowHelper.ThrowInvalidOperationException(message);
        }
    }

    /// <summary>
    ///     Throws an <see cref="InvalidOperationException" /> if called from the UI thread.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public static void ThrowIfOnUIThread(string message)
    {
        if (JoinableTaskContext?.IsOnMainThread ?? false)
        {
            ThrowHelper.ThrowInvalidOperationException(message);
        }
    }

    /// <summary>
    ///     Ignores the result of the specified asynchronous operation.
    /// </summary>
    /// <param name="task">The asynchronous operation to ignore.</param>
    [SuppressMessage("Usage",
                     "VSTHRD100:Avoid async void methods",
                     Justification =
                         "This is done purposefully so that if an async relay command is invoked synchronously, exceptions in the wrapped delegate will not be ignored.")]
    internal static async void Forget(this ValueTask task)
    {
        // Note: This method is purposefully an async void method awaiting the input task. This is done so that
        // if an async relay command is invoked synchronously (ie. when ThreadHelper is called, eg. from a binding),
        // exceptions in the wrapped delegate will not be ignored or just become visible through the ExecutionTask
        // property, but will be rethrown in the original synchronization context by default. This makes the behavior
        // more consistent with how normal commands work (where exceptions are also just normally propagated to the
        // caller context), and avoids getting an app into an inconsistent state in case a method faults without
        // other components being notified.
        await task.Preserve().ConfigureAwait(true);
    }

    /// <summary>
    ///     Ignores the result of the specified asynchronous operation.
    /// </summary>
    /// <typeparam name="T">The result type of the asynchronous operation.</typeparam>
    /// <param name="task">The asynchronous operation to ignore.</param>
    [SuppressMessage("Usage",
                     "VSTHRD100:Avoid async void methods",
                     Justification =
                         "This is done purposefully so that if an async relay command is invoked synchronously, exceptions in the wrapped delegate will not be ignored.")]
    internal static async void Forget<T>(this ValueTask<T> task)
    {
        // Note: This method is purposefully an async void method awaiting the input task. This is done so that
        // if an async relay command is invoked synchronously (ie. when ThreadHelper is called, eg. from a binding),
        // exceptions in the wrapped delegate will not be ignored or just become visible through the ExecutionTask
        // property, but will be rethrown in the original synchronization context by default. This makes the behavior
        // more consistent with how normal commands work (where exceptions are also just normally propagated to the
        // caller context), and avoids getting an app into an inconsistent state in case a method faults without
        // other components being notified.
        await task.Preserve().ConfigureAwait(true);
    }

    private static void OnCurrentExit(object sender, ExitEventArgs e)
    {
        if (!DisposalToken.IsCancellationRequested)
        {
            CleanUp();
        }
    }

    private static void ThrowIfDisposed()
    {
        if (DisposalToken.IsCancellationRequested)
        {
            ThrowHelper.ThrowInvalidOperationException(
                $"{nameof(ThreadHelper)} class was used after being cleaned up.");
        }
    }
}