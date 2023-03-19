using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Enables easy marshalling of code to the UI thread.</summary>
[PublicAPI]
public static class ThreadHelper
{
    private static CancellationTokenRegistration? _cleanupRegistration;
    private static CancellationTokenSource _disposeCancellationTokenSource = new();

    private static JoinableTaskCollection? _joinableTaskCollection;
    private static JoinableTaskContext? _joinableTaskContext;
    private static JoinableTaskFactory? _joinableTaskFactory;

    /// <summary>Gets a value indicating whether the called is on the UI thread.</summary>
    /// <value><see langword="true"/> if the called is on the UI thread; otherwise, <see langword="false"/>.</value>
    public static bool IsOnUIThread => JoinableTaskContext.IsOnMainThread;

    private static JoinableTaskContext JoinableTaskContext => _joinableTaskContext ?? new JoinableTaskContext();

    private static JoinableTaskFactory JoinableTaskFactory => _joinableTaskFactory ?? JoinableTaskContext.Factory;

    /// <summary>
    ///     Provides a <see cref="JoinableTaskContext"/> representing the main thread synchronization context and a
    ///     <see cref="CancellationToken"/> which provides the events to trigger cleanup of the <see cref="ThreadHelper"/>.
    /// </summary>
    /// <param name="mainThreadContext">The <see cref="JoinableTaskContext"/> which operates on the main thread.</param>
    /// <param name="cleanupTriggerToken">
    ///     A <see cref="CancellationToken"/> which, when cancelled, will trigger the cleanup
    ///     process.
    /// </param>
    /// <param name="cleanupTimeout">
    ///     (Optional) The <see cref="TimeSpan"/> to wait before canceling any remaining tasks. The
    ///     default value is zero.
    /// </param>
    /// <param name="exceptionHandler">
    ///     (Optional) The handler for any exceptions that are caught during cleanup. The default
    ///     value is <see langword="null"/>.
    /// </param>
    public static void Initialize(JoinableTaskContext mainThreadContext,
                                  CancellationToken cleanupTriggerToken,
                                  TimeSpan cleanupTimeout = default,
                                  Action<Exception>? exceptionHandler = default)
    {
        CleanUp();

        _disposeCancellationTokenSource = new CancellationTokenSource();
        _cleanupRegistration = cleanupTriggerToken.Register(() => CleanUp(cleanupTimeout, exceptionHandler));

        _joinableTaskContext = mainThreadContext;
        _joinableTaskCollection = _joinableTaskContext.CreateCollection();
        _joinableTaskFactory = _joinableTaskContext.CreateFactory(_joinableTaskCollection);
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
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    [SuppressMessage(category: "Usage",
                     checkId: "VSTHRD002:Avoid problematic synchronous waits",
                     Justification = "Only used for uninitialized state, which is mostly testing.")]
    public static void RunOnBackgroundThread(this Action operation,
                                             JoinableTaskCreationOptions creationOptions =
                                                 JoinableTaskCreationOptions.None,
                                             CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        JoinableTaskFactory.Run(async () =>
                                {
                                    using var cts =
                                        CancellationTokenSource.CreateLinkedTokenSource(
                                            _disposeCancellationTokenSource.Token,
                                            cancellationToken);

                                    await Task.Yield();

                                    // Switch to background thread.
                                    await TaskScheduler.Default;
                                    cts.Token.ThrowIfCancellationRequested();

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
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    [SuppressMessage(category: "Usage",
                     checkId: "VSTHRD002:Avoid problematic synchronous waits",
                     Justification = "Only used for uninitialized state, which is mostly testing.")]
    public static void RunOnBackgroundThread(this Func<CancellationToken, ValueTask> operation,
                                             JoinableTaskCreationOptions creationOptions =
                                                 JoinableTaskCreationOptions.None,
                                             CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        JoinableTaskFactory.Run(async () =>
                                {
                                    using var cts =
                                        CancellationTokenSource.CreateLinkedTokenSource(
                                            _disposeCancellationTokenSource.Token,
                                            cancellationToken);

                                    await Task.Yield();

                                    // Switch to background thread.
                                    await TaskScheduler.Default;
                                    cts.Token.ThrowIfCancellationRequested();

                                    await operation(cts.Token);
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
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>The result of the asynchronous operation.</returns>
    [SuppressMessage(category: "Correctness",
                     checkId: "SS034:Use await to get the result of an asynchronous operation",
                     Justification = "Only used for uninitialized state, which is mostly testing.")]
    [SuppressMessage(category: "Usage",
                     checkId: "VSTHRD002:Avoid problematic synchronous waits",
                     Justification = "Only used for uninitialized state, which is mostly testing.")]
    public static T RunOnBackgroundThread<T>(this Func<CancellationToken, ValueTask<T>> operation,
                                             JoinableTaskCreationOptions creationOptions =
                                                 JoinableTaskCreationOptions.None,
                                             CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        return JoinableTaskFactory.Run(async () =>
                                       {
                                           using var cts =
                                               CancellationTokenSource.CreateLinkedTokenSource(
                                                   _disposeCancellationTokenSource.Token,
                                                   cancellationToken);

                                           await Task.Yield();

                                           // Switch to background thread.
                                           await TaskScheduler.Default;
                                           cts.Token.ThrowIfCancellationRequested();

                                           return await operation(cts.Token);
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
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>The result of the operation.</returns>
    [SuppressMessage(category: "Correctness",
                     checkId: "SS034:Use await to get the result of an asynchronous operation",
                     Justification = "Only used for uninitialized state, which is mostly testing.")]
    [SuppressMessage(category: "Usage",
                     checkId: "VSTHRD002:Avoid problematic synchronous waits",
                     Justification = "Only used for uninitialized state, which is mostly testing.")]
    public static T RunOnBackgroundThread<T>(this Func<T> operation,
                                             JoinableTaskCreationOptions creationOptions =
                                                 JoinableTaskCreationOptions.None,
                                             CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        return JoinableTaskFactory.Run(async () =>
                                       {
                                           using var cts =
                                               CancellationTokenSource.CreateLinkedTokenSource(
                                                   _disposeCancellationTokenSource.Token,
                                                   cancellationToken);

                                           await Task.Yield();

                                           // Switch to background thread.
                                           await TaskScheduler.Default;
                                           cts.Token.ThrowIfCancellationRequested();

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
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    public static void RunOnBackgroundThreadAndForget(this Action operation,
                                                      JoinableTaskCreationOptions creationOptions =
                                                          JoinableTaskCreationOptions.None,
                                                      CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        operation.RunOnBackgroundThreadAsync(creationOptions, cancellationToken).Forget();
    }

    /// <summary>Executes the specified asynchronous operation on a background thread and ignores the result.</summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    public static void RunOnBackgroundThreadAndForget(this Func<CancellationToken, ValueTask> operation,
                                                      JoinableTaskCreationOptions creationOptions =
                                                          JoinableTaskCreationOptions.None,
                                                      CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        operation.RunOnBackgroundThreadAsync(creationOptions, cancellationToken).Forget();
    }

    /// <summary>Executes the specified asynchronous operation on a background thread and ignores the result.</summary>
    /// <typeparam name="T">The result type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    public static void RunOnBackgroundThreadAndForget<T>(this Func<CancellationToken, ValueTask<T>> operation,
                                                         JoinableTaskCreationOptions creationOptions =
                                                             JoinableTaskCreationOptions.None,
                                                         CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        operation.RunOnBackgroundThreadAsync(creationOptions, cancellationToken).Forget();
    }

    /// <summary>Executes the specified operation asynchronously on a background thread and ignores the result.</summary>
    /// <typeparam name="T">The result type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    public static void RunOnBackgroundThreadAndForget<T>(this Func<T> operation,
                                                         JoinableTaskCreationOptions creationOptions =
                                                             JoinableTaskCreationOptions.None,
                                                         CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        operation.RunOnBackgroundThreadAsync(creationOptions, cancellationToken).Forget();
    }

    /// <summary>Executes the specified operation asynchronously on a background thread.</summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async ValueTask RunOnBackgroundThreadAsync(this Action operation,
                                                             JoinableTaskCreationOptions creationOptions =
                                                                 JoinableTaskCreationOptions.None,
                                                             CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await JoinableTaskFactory.RunAsync(async () =>
                                           {
                                               using var cts =
                                                   CancellationTokenSource.CreateLinkedTokenSource(
                                                       _disposeCancellationTokenSource.Token,
                                                       cancellationToken);

                                               await Task.Yield();

                                               // Switch to background thread.
                                               await TaskScheduler.Default;
                                               cts.Token.ThrowIfCancellationRequested();

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
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async ValueTask RunOnBackgroundThreadAsync(this Func<CancellationToken, ValueTask> operation,
                                                             JoinableTaskCreationOptions creationOptions =
                                                                 JoinableTaskCreationOptions.None,
                                                             CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await JoinableTaskFactory.RunAsync(async () =>
                                           {
                                               using var cts =
                                                   CancellationTokenSource.CreateLinkedTokenSource(
                                                       _disposeCancellationTokenSource.Token,
                                                       cancellationToken);

                                               await Task.Yield();

                                               // Switch to background thread.
                                               await TaskScheduler.Default;
                                               cts.Token.ThrowIfCancellationRequested();

                                               await operation(cts.Token);
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
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async ValueTask<T> RunOnBackgroundThreadAsync<T>(this Func<CancellationToken, ValueTask<T>> operation,
                                                                   JoinableTaskCreationOptions creationOptions =
                                                                       JoinableTaskCreationOptions.None,
                                                                   CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        return await JoinableTaskFactory.RunAsync(async () =>
                                                  {
                                                      using var cts =
                                                          CancellationTokenSource.CreateLinkedTokenSource(
                                                              _disposeCancellationTokenSource.Token,
                                                              cancellationToken);

                                                      await Task.Yield();

                                                      // Switch to background thread.
                                                      await TaskScheduler.Default;
                                                      cts.Token.ThrowIfCancellationRequested();

                                                      return await operation(cts.Token);
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
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the operation.</returns>
    public static async ValueTask<T> RunOnBackgroundThreadAsync<T>(this Func<T> operation,
                                                                   JoinableTaskCreationOptions creationOptions =
                                                                       JoinableTaskCreationOptions.None,
                                                                   CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        return await JoinableTaskFactory.RunAsync(async () =>
                                                  {
                                                      using var cts =
                                                          CancellationTokenSource.CreateLinkedTokenSource(
                                                              _disposeCancellationTokenSource.Token,
                                                              cancellationToken);

                                                      await Task.Yield();

                                                      // Switch to background thread.
                                                      await TaskScheduler.Default;
                                                      cts.Token.ThrowIfCancellationRequested();

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
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    public static void RunOnUIThread(this Action operation,
                                     JoinableTaskCreationOptions creationOptions = JoinableTaskCreationOptions.None,
                                     CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        JoinableTaskFactory.Run(async () =>
                                {
                                    using var cts =
                                        CancellationTokenSource.CreateLinkedTokenSource(
                                            _disposeCancellationTokenSource.Token,
                                            cancellationToken);

                                    await Task.Yield();

                                    // Switch to UI thread.
                                    await JoinableTaskFactory.SwitchToMainThreadAsync(cts.Token);
                                    cts.Token.ThrowIfCancellationRequested();

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
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    [SuppressMessage(category: "Usage",
                     checkId: "VSTHRD002:Avoid problematic synchronous waits",
                     Justification = "Only used for uninitialized state, which is mostly testing.")]
    public static void RunOnUIThread(this Func<CancellationToken, ValueTask> operation,
                                     JoinableTaskCreationOptions creationOptions = JoinableTaskCreationOptions.None,
                                     CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        JoinableTaskFactory.Run(async () =>
                                {
                                    using var cts =
                                        CancellationTokenSource.CreateLinkedTokenSource(
                                            _disposeCancellationTokenSource.Token,
                                            cancellationToken);

                                    await Task.Yield();

                                    // Switch to UI thread.
                                    await JoinableTaskFactory.SwitchToMainThreadAsync(cts.Token);
                                    cts.Token.ThrowIfCancellationRequested();

                                    await operation(cts.Token);
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
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>The result of the asynchronous operation.</returns>
    [SuppressMessage(category: "Correctness",
                     checkId: "SS034:Use await to get the result of an asynchronous operation",
                     Justification = "Only used for uninitialized state, which is mostly testing.")]
    [SuppressMessage(category: "Usage",
                     checkId: "VSTHRD002:Avoid problematic synchronous waits",
                     Justification = "Only used for uninitialized state, which is mostly testing.")]
    public static T RunOnUIThread<T>(this Func<CancellationToken, ValueTask<T>> operation,
                                     JoinableTaskCreationOptions creationOptions = JoinableTaskCreationOptions.None,
                                     CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        return JoinableTaskFactory.Run(async () =>
                                       {
                                           using var cts =
                                               CancellationTokenSource.CreateLinkedTokenSource(
                                                   _disposeCancellationTokenSource.Token,
                                                   cancellationToken);

                                           await Task.Yield();

                                           // Switch to UI thread.
                                           await JoinableTaskFactory.SwitchToMainThreadAsync(cts.Token);
                                           cts.Token.ThrowIfCancellationRequested();

                                           return await operation(cts.Token);
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
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>The result of the operation.</returns>
    public static T RunOnUIThread<T>(this Func<T> operation,
                                     JoinableTaskCreationOptions creationOptions = JoinableTaskCreationOptions.None,
                                     CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        return JoinableTaskFactory.Run(async () =>
                                       {
                                           using var cts =
                                               CancellationTokenSource.CreateLinkedTokenSource(
                                                   _disposeCancellationTokenSource.Token,
                                                   cancellationToken);

                                           await Task.Yield();

                                           // Switch to UI thread.
                                           await JoinableTaskFactory.SwitchToMainThreadAsync(cts.Token);
                                           cts.Token.ThrowIfCancellationRequested();

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
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    public static void RunOnUIThreadAndForget(this Action operation,
                                              JoinableTaskCreationOptions creationOptions =
                                                  JoinableTaskCreationOptions.None,
                                              CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        using var relevance = JoinableTaskContext.SuppressRelevance();

        operation.RunOnUIThreadAsync(creationOptions, cancellationToken).Forget();
    }

    /// <summary>Executes the specified asynchronous operation on the UI thread and ignores the result.</summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    public static void RunOnUIThreadAndForget(this Func<CancellationToken, ValueTask> operation,
                                              JoinableTaskCreationOptions creationOptions =
                                                  JoinableTaskCreationOptions.None,
                                              CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        using var relevance = JoinableTaskContext.SuppressRelevance();

        operation.RunOnUIThreadAsync(creationOptions, cancellationToken).Forget();
    }

    /// <summary>Executes the specified asynchronous operation on the UI thread and ignores the result.</summary>
    /// <typeparam name="T">The result type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    public static void RunOnUIThreadAndForget<T>(this Func<CancellationToken, ValueTask<T>> operation,
                                                 JoinableTaskCreationOptions creationOptions =
                                                     JoinableTaskCreationOptions.None,
                                                 CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        using var relevance = JoinableTaskContext.SuppressRelevance();

        operation.RunOnUIThreadAsync(creationOptions, cancellationToken).Forget();
    }

    /// <summary>Executes the specified operation asynchronously on the UI thread and ignores the result.</summary>
    /// <typeparam name="T">The result type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    public static void RunOnUIThreadAndForget<T>(this Func<T> operation,
                                                 JoinableTaskCreationOptions creationOptions =
                                                     JoinableTaskCreationOptions.None,
                                                 CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        using var relevance = JoinableTaskContext.SuppressRelevance();

        operation.RunOnUIThreadAsync(creationOptions, cancellationToken).Forget();
    }

    /// <summary>Executes the specified operation asynchronously on the UI thread.</summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="creationOptions">
    ///     (Optional) The <see cref="JoinableTaskCreationOptions"/> used to customize the task's
    ///     behavior.
    /// </param>
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async ValueTask RunOnUIThreadAsync(this Action operation,
                                                     JoinableTaskCreationOptions creationOptions =
                                                         JoinableTaskCreationOptions.None,
                                                     CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await JoinableTaskFactory.RunAsync(async () =>
                                           {
                                               using var cts =
                                                   CancellationTokenSource.CreateLinkedTokenSource(
                                                       _disposeCancellationTokenSource.Token,
                                                       cancellationToken);

                                               await Task.Yield();

                                               // Switch to UI thread.
                                               await JoinableTaskFactory.SwitchToMainThreadAsync(cts.Token);
                                               cts.Token.ThrowIfCancellationRequested();

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
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async ValueTask RunOnUIThreadAsync(this Func<CancellationToken, ValueTask> operation,
                                                     JoinableTaskCreationOptions creationOptions =
                                                         JoinableTaskCreationOptions.None,
                                                     CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await JoinableTaskFactory.RunAsync(async () =>
                                           {
                                               using var cts =
                                                   CancellationTokenSource.CreateLinkedTokenSource(
                                                       _disposeCancellationTokenSource.Token,
                                                       cancellationToken);

                                               await Task.Yield();

                                               // Switch to UI thread.
                                               await JoinableTaskFactory.SwitchToMainThreadAsync(cts.Token);
                                               cts.Token.ThrowIfCancellationRequested();

                                               await operation(cts.Token);
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
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the operation.</returns>
    public static async ValueTask<T> RunOnUIThreadAsync<T>(this Func<CancellationToken, ValueTask<T>> operation,
                                                           JoinableTaskCreationOptions creationOptions =
                                                               JoinableTaskCreationOptions.None,
                                                           CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        return await JoinableTaskFactory.RunAsync(async () =>
                                                  {
                                                      using var cts =
                                                          CancellationTokenSource.CreateLinkedTokenSource(
                                                              _disposeCancellationTokenSource.Token,
                                                              cancellationToken);

                                                      await Task.Yield();

                                                      // Switch to UI thread.
                                                      await JoinableTaskFactory.SwitchToMainThreadAsync(cts.Token);
                                                      cts.Token.ThrowIfCancellationRequested();

                                                      return await operation(cts.Token);
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
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the operation.</returns>
    public static async ValueTask<T> RunOnUIThreadAsync<T>(this Func<T> operation,
                                                           JoinableTaskCreationOptions creationOptions =
                                                               JoinableTaskCreationOptions.None,
                                                           CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        return await JoinableTaskFactory.RunAsync(async () =>
                                                  {
                                                      using var cts =
                                                          CancellationTokenSource.CreateLinkedTokenSource(
                                                              _disposeCancellationTokenSource.Token,
                                                              cancellationToken);

                                                      await Task.Yield();

                                                      // Switch to UI thread.
                                                      await JoinableTaskFactory.SwitchToMainThreadAsync(cts.Token);
                                                      cts.Token.ThrowIfCancellationRequested();

                                                      return operation();
                                                  },
                                                  creationOptions);
    }

    /// <summary>Throws an <see cref="InvalidOperationException"/> if not called from the UI thread.</summary>
    /// <param name="message">(Optional) The exception message.</param>
    public static void ThrowIfNotOnUIThread(string? message = default)
    {
        if (!IsOnUIThread)
        {
            ThrowHelper.ThrowInvalidOperationException(message ?? "The operation needs to run on the main thread.");
        }
    }

    /// <summary>Throws an <see cref="InvalidOperationException"/> if called from the UI thread.</summary>
    /// <param name="message">(Optional) The exception message.</param>
    public static void ThrowIfOnUIThread(string? message = default)
    {
        if (IsOnUIThread)
        {
            ThrowHelper.ThrowInvalidOperationException(message ?? "The operation must not run on the main thread.");
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

    private static void CleanUp(TimeSpan cleanupTimeout = default, Action<Exception>? exceptionHandler = default)
    {
        if (_disposeCancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        _disposeCancellationTokenSource.TryCancel();

        try
        {
            using var cts = new CancellationTokenSource();

            var token = cts.Token;

            cts.CancelAfter(cleanupTimeout);

            JoinableTaskContext.Factory.Run(() => _joinableTaskCollection?.JoinTillEmptyAsync(token) ??
                                                  Task.CompletedTask);
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
            JoinableTaskContext.Dispose();
            _disposeCancellationTokenSource.Dispose();
            _cleanupRegistration?.Dispose();
            _cleanupRegistration = null;
            _joinableTaskFactory = null;
            _joinableTaskCollection = null;
            _joinableTaskContext = null;
        }
    }

    private static void ThrowIfDisposed()
    {
        if (_disposeCancellationTokenSource.IsCancellationRequested)
        {
            ThrowHelper.ThrowInvalidOperationException(
                $"{nameof(ThreadHelper)} class was used after being cleaned up.");
        }
    }
}