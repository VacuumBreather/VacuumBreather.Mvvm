﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Threading;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Enables easy marshalling of code to the UI thread.</summary>
[PublicAPI]
public static class ThreadHelper
{
    /// <summary>Gets a value indicating whether the called is on the UI thread.</summary>
    /// <value><see langword="true"/> if the called is on the UI thread; otherwise, <see langword="false"/>.</value>
    public static bool IsOnUIThread => !CanUseDispatcher || (IsInitialized && JoinableTaskContext!.IsOnMainThread);

    private static bool CanUseDispatcher { get; set; }

    private static Dispatcher? Dispatcher { get; set; }

    private static bool IsInitialized { get; set; }

    private static JoinableTaskCollection? JoinableTaskCollection { get; set; }

    private static JoinableTaskContext? JoinableTaskContext { get; set; }

    private static JoinableTaskFactory? JoinableTaskFactory { get; set; }

    /// <summary>Cleans up all unfinished async work. Consecutive calls will have no effect.</summary>
    /// <param name="cleanupTimeout">
    ///     (Optional) The <see cref="TimeSpan"/> to wait before canceling any remaining tasks. The
    ///     default value is zero.
    /// </param>
    /// <param name="exceptionHandler">
    ///     (Optional) The handler for any exceptions that are caught during cleanup. The default
    ///     value is <see langword="null"/>.
    /// </param>
    public static void CleanUp(TimeSpan cleanupTimeout = default, Action<Exception>? exceptionHandler = default)
    {
        if (!IsInitialized)
        {
            return;
        }

        IsInitialized = false;

        try
        {
            if (JoinableTaskCollection is not null)
            {
                using var cts = new CancellationTokenSource();
                using var context = new JoinableTaskContext(Dispatcher!.Thread);

                var token = cts.Token;
                var taskFactory = new JoinableTaskFactory(context);

                cts.CancelAfter(cleanupTimeout);
                taskFactory.Run(() => JoinableTaskCollection.JoinTillEmptyAsync(token));
            }
        }
        catch (OperationCanceledException)
        {
            // This exception is expected because we signaled the cancellation token
        }
        catch (AggregateException exception)
        {
            try
            {
                // Ignore AggregateException containing only OperationCanceledException
                exception.Handle(inner => inner is OperationCanceledException);
            }
            catch (AggregateException aggregateException)
            {
                exceptionHandler?.Invoke(aggregateException);
            }
        }
        catch (Exception ex) when (exceptionHandler is not null)
        {
            exceptionHandler.Invoke(ex);
        }
        finally
        {
            JoinableTaskContext?.Dispose();
            JoinableTaskContext = null;
            JoinableTaskCollection = null;
            JoinableTaskFactory = null;
            Dispatcher = null;
        }
    }

    /// <summary>
    ///     Passes a <see cref="IHostApplicationLifetime"/> which provides the events to trigger cleanup of the
    ///     <see cref="ThreadHelper"/>. Consecutive calls will have no effect.
    /// </summary>
    /// <param name="mainThreadDispatcher">The main thread dispatcher.</param>
    /// <param name="applicationLifetime">The application lifetime to trigger the cleanup process.</param>
    /// <param name="cleanupTimeout">
    ///     (Optional) The <see cref="TimeSpan"/> to wait before canceling any remaining tasks. The
    ///     default value is zero.
    /// </param>
    /// <param name="exceptionHandler">
    ///     (Optional) The handler for any exceptions that are caught during cleanup. The default
    ///     value is <see langword="null"/>.
    /// </param>
    public static void Initialize(Dispatcher mainThreadDispatcher,
                                  IHostApplicationLifetime applicationLifetime,
                                  TimeSpan cleanupTimeout = default,
                                  Action<Exception>? exceptionHandler = default)
    {
        if (IsInitialized)
        {
            return;
        }

        applicationLifetime.ApplicationStopping.Register(() => CleanUp(cleanupTimeout, exceptionHandler));

        Dispatcher = mainThreadDispatcher;

        JoinableTaskContext?.Dispose();

        JoinableTaskContext =
            new JoinableTaskContext(Dispatcher.Thread, new DispatcherSynchronizationContext(Dispatcher));

        JoinableTaskCollection = JoinableTaskContext.CreateCollection();
        JoinableTaskFactory = JoinableTaskContext.CreateFactory(JoinableTaskCollection);

        CanUseDispatcher = true;
        IsInitialized = true;
    }

    /// <summary>
    ///     Executes the specified operation on a background thread and runs it to completion while synchronously blocking
    ///     the calling thread.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    [SuppressMessage(category: "Usage",
                     checkId: "VSTHRD002:Avoid problematic synchronous waits",
                     Justification = "Only used for uninitialized state, which is mostly testing.")]
    public static void RunOnBackgroundThread(this Action operation,
                                             JoinableTaskCreationOptions creationOptions =
                                                 JoinableTaskCreationOptions.None)
    {
        if (!CanUseDispatcher)
        {
            Task.Run(operation).Wait();

            return;
        }

        ThrowIfDisposed();

        JoinableTaskFactory!.Run(async () =>
                                 {
                                     // Switch to background thread.
                                     await TaskScheduler.Default;

                                     operation();
                                 },
                                 creationOptions);
    }

    /// <summary>
    ///     Executes the specified asynchronous operation on a background thread and runs it to completion while
    ///     synchronously blocking the calling thread.
    /// </summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    [SuppressMessage(category: "Usage",
                     checkId: "VSTHRD002:Avoid problematic synchronous waits",
                     Justification = "Only used for uninitialized state, which is mostly testing.")]
    public static void RunOnBackgroundThread(this Func<ValueTask> operation,
                                             JoinableTaskCreationOptions creationOptions =
                                                 JoinableTaskCreationOptions.None)
    {
        if (!CanUseDispatcher)
        {
            Task.Run(async () => await operation()).Wait();

            return;
        }

        ThrowIfDisposed();

        JoinableTaskFactory!.Run(async () =>
                                 {
                                     // Switch to background thread.
                                     await TaskScheduler.Default;

                                     await operation();
                                 },
                                 creationOptions);
    }

    /// <summary>
    ///     Executes the specified asynchronous operation on a background thread and runs it to completion while
    ///     synchronously blocking the calling thread.
    /// </summary>
    /// <typeparam name="T">The result type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>The result of the asynchronous operation.</returns>
    [SuppressMessage(category: "Correctness",
                     checkId: "SS034:Use await to get the result of an asynchronous operation",
                     Justification = "Only used for uninitialized state, which is mostly testing.")]
    [SuppressMessage(category: "Usage",
                     checkId: "VSTHRD002:Avoid problematic synchronous waits",
                     Justification = "Only used for uninitialized state, which is mostly testing.")]
    public static T RunOnBackgroundThread<T>(this Func<ValueTask<T>> operation,
                                             JoinableTaskCreationOptions creationOptions =
                                                 JoinableTaskCreationOptions.None)
    {
        if (!CanUseDispatcher)
        {
            return Task.Run(async () => await operation()).Result;
        }

        ThrowIfDisposed();

        return JoinableTaskFactory!.Run(async () =>
                                        {
                                            // Switch to background thread.
                                            await TaskScheduler.Default;

                                            return await operation();
                                        },
                                        creationOptions);
    }

    /// <summary>
    ///     Executes the specified operation on a background thread and runs it to completion while synchronously blocking
    ///     the calling thread.
    /// </summary>
    /// <typeparam name="T">The result type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>The result of the operation.</returns>
    [SuppressMessage(category: "Correctness",
                     checkId: "SS034:Use await to get the result of an asynchronous operation",
                     Justification = "Only used for uninitialized state, which is mostly testing.")]
    [SuppressMessage(category: "Usage",
                     checkId: "VSTHRD002:Avoid problematic synchronous waits",
                     Justification = "Only used for uninitialized state, which is mostly testing.")]
    public static T RunOnBackgroundThread<T>(this Func<T> operation,
                                             JoinableTaskCreationOptions creationOptions =
                                                 JoinableTaskCreationOptions.None)
    {
        if (!CanUseDispatcher)
        {
            return Task.Run(operation).Result;
        }

        ThrowIfDisposed();

        return JoinableTaskFactory!.Run(async () =>
                                        {
                                            // Switch to background thread.
                                            await TaskScheduler.Default;

                                            return operation();
                                        },
                                        creationOptions);
    }

    /// <summary>Executes the specified operation asynchronously on a background thread and ignores the result.</summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnBackgroundThreadAndForget(this Action operation,
                                                      JoinableTaskCreationOptions creationOptions =
                                                          JoinableTaskCreationOptions.None)
    {
        if (CanUseDispatcher)
        {
            ThrowIfDisposed();
        }

        operation.RunOnBackgroundThreadAsync(creationOptions).Forget();
    }

    /// <summary>Executes the specified asynchronous operation on a background thread and ignores the result.</summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnBackgroundThreadAndForget(this Func<ValueTask> operation,
                                                      JoinableTaskCreationOptions creationOptions =
                                                          JoinableTaskCreationOptions.None)
    {
        if (CanUseDispatcher)
        {
            ThrowIfDisposed();
        }

        operation.RunOnBackgroundThreadAsync(creationOptions).Forget();
    }

    /// <summary>Executes the specified asynchronous operation on a background thread and ignores the result.</summary>
    /// <typeparam name="T">The result type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnBackgroundThreadAndForget<T>(this Func<ValueTask<T>> operation,
                                                         JoinableTaskCreationOptions creationOptions =
                                                             JoinableTaskCreationOptions.None)
    {
        if (CanUseDispatcher)
        {
            ThrowIfDisposed();
        }

        operation.RunOnBackgroundThreadAsync(creationOptions).Forget();
    }

    /// <summary>Executes the specified operation asynchronously on a background thread and ignores the result.</summary>
    /// <typeparam name="T">The result type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnBackgroundThreadAndForget<T>(this Func<T> operation,
                                                         JoinableTaskCreationOptions creationOptions =
                                                             JoinableTaskCreationOptions.None)
    {
        if (CanUseDispatcher)
        {
            ThrowIfDisposed();
        }

        operation.RunOnBackgroundThreadAsync(creationOptions).Forget();
    }

    /// <summary>Executes the specified operation asynchronously on a background thread.</summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async ValueTask RunOnBackgroundThreadAsync(this Action operation,
                                                             JoinableTaskCreationOptions creationOptions =
                                                                 JoinableTaskCreationOptions.None)
    {
        if (!CanUseDispatcher)
        {
            await Task.Run(operation);

            return;
        }

        ThrowIfDisposed();

        await JoinableTaskFactory!.RunAsync(async () =>
                                            {
                                                // Switch to background thread.
                                                await TaskScheduler.Default;

                                                operation();
                                            },
                                            creationOptions);
    }

    /// <summary>Executes the specified asynchronous operation on a background thread.</summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async ValueTask RunOnBackgroundThreadAsync(this Func<ValueTask> operation,
                                                             JoinableTaskCreationOptions creationOptions =
                                                                 JoinableTaskCreationOptions.None)
    {
        if (!CanUseDispatcher)
        {
            await Task.Run(async () => await operation());

            return;
        }

        ThrowIfDisposed();

        await JoinableTaskFactory!.RunAsync(async () =>
                                            {
                                                // Switch to background thread.
                                                await TaskScheduler.Default;

                                                await operation();
                                            },
                                            creationOptions);
    }

    /// <summary>Executes the specified asynchronous operation on a background thread.</summary>
    /// <typeparam name="T">The result type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async ValueTask<T> RunOnBackgroundThreadAsync<T>(this Func<ValueTask<T>> operation,
                                                                   JoinableTaskCreationOptions creationOptions =
                                                                       JoinableTaskCreationOptions.None)
    {
        if (!CanUseDispatcher)
        {
            return await Task.Run(async () => await operation());
        }

        ThrowIfDisposed();

        return await JoinableTaskFactory!.RunAsync(async () =>
                                                   {
                                                       // Switch to background thread.
                                                       await TaskScheduler.Default;

                                                       return await operation();
                                                   },
                                                   creationOptions);
    }

    /// <summary>Executes the specified operation asynchronously on a background thread.</summary>
    /// <typeparam name="T">The result type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the operation.</returns>
    public static async ValueTask<T> RunOnBackgroundThreadAsync<T>(this Func<T> operation,
                                                                   JoinableTaskCreationOptions creationOptions =
                                                                       JoinableTaskCreationOptions.None)
    {
        if (!CanUseDispatcher)
        {
            return await Task.Run(operation);
        }

        ThrowIfDisposed();

        return await JoinableTaskFactory!.RunAsync(async () =>
                                                   {
                                                       // Switch to background thread.
                                                       await TaskScheduler.Default;

                                                       return operation();
                                                   },
                                                   creationOptions);
    }

    /// <summary>
    ///     Executes the specified operation on the UI thread and runs it to completion while synchronously blocking the
    ///     calling thread.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnUIThread(this Action operation,
                                     JoinableTaskCreationOptions creationOptions = JoinableTaskCreationOptions.None)
    {
        if (!CanUseDispatcher)
        {
            operation();

            return;
        }

        ThrowIfDisposed();

        JoinableTaskFactory!.Run(async () =>
                                 {
                                     // Switch to UI thread.
                                     await JoinableTaskFactory.SwitchToMainThreadAsync();

                                     operation();
                                 },
                                 creationOptions);
    }

    /// <summary>
    ///     Executes the specified asynchronous operation on the UI thread and runs it to completion while synchronously
    ///     blocking the calling thread.
    /// </summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    [SuppressMessage(category: "Usage",
                     checkId: "VSTHRD002:Avoid problematic synchronous waits",
                     Justification = "Only used for uninitialized state, which is mostly testing.")]
    public static void RunOnUIThread(this Func<ValueTask> operation,
                                     JoinableTaskCreationOptions creationOptions = JoinableTaskCreationOptions.None)
    {
        if (!CanUseDispatcher)
        {
            Task.Run(async () => await operation()).Wait();

            return;
        }

        ThrowIfDisposed();

        JoinableTaskFactory!.Run(async () =>
                                 {
                                     // Switch to UI thread.
                                     await JoinableTaskFactory.SwitchToMainThreadAsync();
                                     await operation();
                                 },
                                 creationOptions);
    }

    /// <summary>
    ///     Executes the specified asynchronous operation on the UI thread and runs it to completion while synchronously
    ///     blocking the calling thread.
    /// </summary>
    /// <typeparam name="T">The result type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>The result of the asynchronous operation.</returns>
    [SuppressMessage(category: "Correctness",
                     checkId: "SS034:Use await to get the result of an asynchronous operation",
                     Justification = "Only used for uninitialized state, which is mostly testing.")]
    [SuppressMessage(category: "Usage",
                     checkId: "VSTHRD002:Avoid problematic synchronous waits",
                     Justification = "Only used for uninitialized state, which is mostly testing.")]
    public static T RunOnUIThread<T>(this Func<ValueTask<T>> operation,
                                     JoinableTaskCreationOptions creationOptions = JoinableTaskCreationOptions.None)
    {
        if (!CanUseDispatcher)
        {
            return Task.Run(async () => await operation()).Result;
        }

        ThrowIfDisposed();

        return JoinableTaskFactory!.Run(async () =>
                                        {
                                            // Switch to UI thread.
                                            await JoinableTaskFactory.SwitchToMainThreadAsync();

                                            return await operation();
                                        },
                                        creationOptions);
    }

    /// <summary>
    ///     Executes the specified operation on the UI thread and runs it to completion while synchronously blocking the
    ///     calling thread.
    /// </summary>
    /// <typeparam name="T">The result type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>The result of the operation.</returns>
    public static T RunOnUIThread<T>(this Func<T> operation,
                                     JoinableTaskCreationOptions creationOptions = JoinableTaskCreationOptions.None)
    {
        if (!CanUseDispatcher)
        {
            return operation();
        }

        ThrowIfDisposed();

        return JoinableTaskFactory!.Run(async () =>
                                        {
                                            // Switch to UI thread.
                                            await JoinableTaskFactory.SwitchToMainThreadAsync();

                                            return operation();
                                        },
                                        creationOptions);
    }

    /// <summary>Executes the specified operation asynchronously on the UI thread and ignores the result.</summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnUIThreadAndForget(this Action operation,
                                              JoinableTaskCreationOptions creationOptions =
                                                  JoinableTaskCreationOptions.None)
    {
        if (CanUseDispatcher)
        {
            ThrowIfDisposed();
        }

        operation.RunOnUIThreadAsync(creationOptions).Forget();
    }

    /// <summary>Executes the specified asynchronous operation on the UI thread and ignores the result.</summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnUIThreadAndForget(this Func<ValueTask> operation,
                                              JoinableTaskCreationOptions creationOptions =
                                                  JoinableTaskCreationOptions.None)
    {
        if (CanUseDispatcher)
        {
            ThrowIfDisposed();
        }

        operation.RunOnUIThreadAsync(creationOptions).Forget();
    }

    /// <summary>Executes the specified asynchronous operation on the UI thread and ignores the result.</summary>
    /// <typeparam name="T">The result type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnUIThreadAndForget<T>(this Func<ValueTask<T>> operation,
                                                 JoinableTaskCreationOptions creationOptions =
                                                     JoinableTaskCreationOptions.None)
    {
        if (CanUseDispatcher)
        {
            ThrowIfDisposed();
        }

        operation.RunOnUIThreadAsync(creationOptions).Forget();
    }

    /// <summary>Executes the specified operation asynchronously on the UI thread and ignores the result.</summary>
    /// <typeparam name="T">The result type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    public static void RunOnUIThreadAndForget<T>(this Func<T> operation,
                                                 JoinableTaskCreationOptions creationOptions =
                                                     JoinableTaskCreationOptions.None)
    {
        if (CanUseDispatcher)
        {
            ThrowIfDisposed();
        }

        operation.RunOnUIThreadAsync(creationOptions).Forget();
    }

    /// <summary>Executes the specified operation asynchronously on the UI thread.</summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async ValueTask RunOnUIThreadAsync(this Action operation,
                                                     JoinableTaskCreationOptions creationOptions =
                                                         JoinableTaskCreationOptions.None)
    {
        if (!CanUseDispatcher)
        {
            await Task.Run(operation);

            return;
        }

        ThrowIfDisposed();

        await JoinableTaskFactory!.RunAsync(async () =>
                                            {
                                                // Switch to UI thread.
                                                await JoinableTaskFactory.SwitchToMainThreadAsync();

                                                operation();
                                            },
                                            creationOptions);
    }

    /// <summary>Executes the specified asynchronous operation on the UI thread.</summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async ValueTask RunOnUIThreadAsync(this Func<ValueTask> operation,
                                                     JoinableTaskCreationOptions creationOptions =
                                                         JoinableTaskCreationOptions.None)
    {
        if (!CanUseDispatcher)
        {
            await Task.Run(async () => await operation());

            return;
        }

        ThrowIfDisposed();

        await JoinableTaskFactory!.RunAsync(async () =>
                                            {
                                                // Switch to UI thread.
                                                await JoinableTaskFactory.SwitchToMainThreadAsync();
                                                await operation();
                                            },
                                            creationOptions);
    }

    /// <summary>Executes the specified asynchronous operation on the UI thread.</summary>
    /// <typeparam name="T">The result type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the operation.</returns>
    public static async ValueTask<T> RunOnUIThreadAsync<T>(this Func<ValueTask<T>> operation,
                                                           JoinableTaskCreationOptions creationOptions =
                                                               JoinableTaskCreationOptions.None)
    {
        if (!CanUseDispatcher)
        {
            return await Task.Run(async () => await operation());
        }

        ThrowIfDisposed();

        return await JoinableTaskFactory!.RunAsync(async () =>
                                                   {
                                                       // Switch to UI thread.
                                                       await JoinableTaskFactory.SwitchToMainThreadAsync();

                                                       return await operation();
                                                   },
                                                   creationOptions);
    }

    /// <summary>Executes the specified operation asynchronously on the UI thread.</summary>
    /// <typeparam name="T">The result type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the operation.</returns>
    public static async ValueTask<T> RunOnUIThreadAsync<T>(this Func<T> operation,
                                                           JoinableTaskCreationOptions creationOptions =
                                                               JoinableTaskCreationOptions.None)
    {
        if (!CanUseDispatcher)
        {
            return await Task.Run(operation);
        }

        ThrowIfDisposed();

        return await JoinableTaskFactory!.RunAsync(async () =>
                                                   {
                                                       // Switch to UI thread.
                                                       await JoinableTaskFactory.SwitchToMainThreadAsync();

                                                       return operation();
                                                   },
                                                   creationOptions);
    }

    /// <summary>Throws an <see cref="InvalidOperationException"/> if not called from the UI thread.</summary>
    /// <param name="message">The exception message.</param>
    public static void ThrowIfNotOnUIThread(string message)
    {
        if (CanUseDispatcher && !IsOnUIThread)
        {
            ThrowHelper.ThrowInvalidOperationException(message);
        }
    }

    /// <summary>Throws an <see cref="InvalidOperationException"/> if called from the UI thread.</summary>
    /// <param name="message">The exception message.</param>
    public static void ThrowIfOnUIThread(string message)
    {
        if (CanUseDispatcher && IsOnUIThread)
        {
            ThrowHelper.ThrowInvalidOperationException(message);
        }
    }

    /// <summary>Ignores the result of the specified asynchronous operation.</summary>
    /// <param name="task">The asynchronous operation to ignore.</param>
    [SuppressMessage(category: "Usage",
                     checkId: "VSTHRD100:Avoid async void methods",
                     Justification =
                         "This is done purposefully so that if an async relay command is invoked synchronously, exceptions in the wrapped delegate will not be ignored.")]
    [SuppressMessage(category: "AsyncUsage",
                     checkId: "AsyncFixer01:Unnecessary async/await usage",
                     Justification = "This is done purposefully.")]
    [SuppressMessage(category: "Correctness",
                     checkId: "SS001:Async methods should return a Task to make them awaitable",
                     Justification = "This is done purposefully.")]
    [SuppressMessage(category: "AsyncUsage",
                     checkId: "AsyncFixer03:Fire-and-forget async-void methods or delegates",
                     Justification = "This is done purposefully.")]
    [SuppressMessage(category: "Major Bug",
                     checkId: "S3168:\"async\" methods should not return \"void\"",
                     Justification = "This is done purposefully.")]
    [SuppressMessage(category: "Correctness",
                     checkId: "SS001:Async methods should return a Task to make them awaitable",
                     Justification = "This is done purposefully.")]
    internal static async void Forget(this ValueTask task)
    {
        // Note: This method is purposefully an async void method awaiting the input task. This is done so that
        // if an async relay command is invoked synchronously (ie. when ThreadHelper is called, eg. from a binding),
        // exceptions in the wrapped delegate will not be ignored or just become visible through the ExecutionTask
        // property, but will be rethrown in the original synchronization context by default. This makes the behavior
        // more consistent with how normal commands work (where exceptions are also just normally propagated to the
        // caller context), and avoids getting an app into an inconsistent state in case a method faults without
        // other components being notified.
        await task.Preserve();
    }

    /// <summary>Ignores the result of the specified asynchronous operation.</summary>
    /// <typeparam name="T">The result type of the asynchronous operation.</typeparam>
    /// <param name="task">The asynchronous operation to ignore.</param>
    [SuppressMessage(category: "Usage",
                     checkId: "VSTHRD100:Avoid async void methods",
                     Justification =
                         "This is done purposefully so that if an async relay command is invoked synchronously, exceptions in the wrapped delegate will not be ignored.")]
    [SuppressMessage(category: "AsyncUsage",
                     checkId: "AsyncFixer01:Unnecessary async/await usage",
                     Justification = "This is done purposefully.")]
    [SuppressMessage(category: "AsyncUsage",
                     checkId: "AsyncFixer03:Fire-and-forget async-void methods or delegates",
                     Justification = "This is done purposefully.")]
    [SuppressMessage(category: "Major Bug",
                     checkId: "S3168:\"async\" methods should not return \"void\"",
                     Justification = "This is done purposefully.")]
    [SuppressMessage(category: "Correctness",
                     checkId: "SS001:Async methods should return a Task to make them awaitable",
                     Justification = "This is done purposefully.")]
    internal static async void Forget<T>(this ValueTask<T> task)
    {
        // Note: This method is purposefully an async void method awaiting the input task. This is done so that
        // if an async relay command is invoked synchronously (ie. when ThreadHelper is called, eg. from a binding),
        // exceptions in the wrapped delegate will not be ignored or just become visible through the ExecutionTask
        // property, but will be rethrown in the original synchronization context by default. This makes the behavior
        // more consistent with how normal commands work (where exceptions are also just normally propagated to the
        // caller context), and avoids getting an app into an inconsistent state in case a method faults without
        // other components being notified.
        await task.Preserve();
    }

    private static void ThrowIfDisposed()
    {
        if (CanUseDispatcher && !IsInitialized)
        {
            ThrowHelper.ThrowInvalidOperationException(
                $"{nameof(ThreadHelper)} class was used after being cleaned up.");
        }
    }
}