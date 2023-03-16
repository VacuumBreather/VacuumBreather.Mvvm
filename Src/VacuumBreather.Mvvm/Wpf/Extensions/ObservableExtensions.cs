using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>Provides extension methods for the <see cref="IObservable{T}"/> type.</summary>
[PublicAPI]
public static class ObservableExtensions
{
    /// <summary>Wraps the source sequence in order to run its observer callbacks on the UI thread.</summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <returns>The source sequence whose observations happen on the UI thread.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <remarks>
    ///     This only invokes observer callbacks on the UI thread. In case the subscription and/or un-subscription actions
    ///     have side-effects that require to be run on the UI thread, use
    ///     <see cref="SubscribeOnUIThread{TSource}(IObservable{TSource})"/>.
    /// </remarks>
    public static IObservable<TSource> ObserveOnUIThread<TSource>(this IObservable<TSource> source)
    {
        return source.ObserveOn(UIThreadScheduler.Instance);
    }

    /// <summary>
    ///     Wraps the source sequence in order to run its subscription and un-subscription logic on the UI thread. This
    ///     operation is not commonly used; see the remarks section for more information on the distinction between SubscribeOn
    ///     and ObserveOn.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <returns>The source sequence whose subscriptions and un-subscriptions happen on the UI thread.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <remarks>
    ///     This only performs the side-effects of subscription and un-subscription on the UI thread. In order to invoke
    ///     observer callbacks on the UI thread, use <see cref="ObserveOnUIThread{TSource}(IObservable{TSource})"/>.
    /// </remarks>
    public static IObservable<TSource> SubscribeOnUIThread<TSource>(this IObservable<TSource> source)
    {
        return source.SubscribeOn(UIThreadScheduler.Instance);
    }

    private sealed class UIThreadScheduler : LocalScheduler, ISchedulerPeriodic
    {
        private UIThreadScheduler()
        {
        }

        /// <summary>Gets the scheduler that schedules work on the UI thread.</summary>
        internal static UIThreadScheduler Instance { get; } = new();

        /// <inheritdoc/>
        public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            Guard.IsNotNull(action);

            var d = new SingleAssignmentDisposable();

            ThreadHelper.RunOnUIThreadAndForget(() =>
            {
                if (!d.IsDisposed)
                {
                    d.Disposable = action(this, state);
                }
            });

            return d;
        }

        /// <inheritdoc/>
        public override IDisposable Schedule<TState>(TState state,
                                                     TimeSpan dueTime,
                                                     Func<IScheduler, TState, IDisposable> action)
        {
            Guard.IsNotNull(action);

            var dt = Scheduler.Normalize(dueTime);

            if (dt.Ticks == 0)
            {
                return Schedule(state, action);
            }

            return ScheduleSlow(state, dt, action);
        }

        /// <inheritdoc/>
        public IDisposable SchedulePeriodic<TState>(TState state, TimeSpan period, Func<TState, TState> action)
        {
            SafeCancellationTokenSource cancellationTokenSource = new();

            var disposableAction = new DisposableAction(() =>
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            });

            ThreadHelper.RunOnUIThreadAndForget(async () =>
            {
                try
                {
                    using var _ = cancellationTokenSource;
                    var changingState = state;

                    while (!cancellationTokenSource.IsCancellationRequested)
                    {
                        await Task.Delay(period, cancellationTokenSource.Token);

                        changingState = action(changingState);
                    }
                }
                catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
                {
                    // Expected cancellation
                }
            });

            return disposableAction;
        }

        private IDisposable ScheduleSlow<TState>(TState state,
                                                 TimeSpan dueTime,
                                                 Func<IScheduler, TState, IDisposable> action)
        {
            var d = new MultipleAssignmentDisposable();

            SafeCancellationTokenSource cancellationTokenSource = new();

            d.Disposable = new DisposableAction(() =>
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            });

            ThreadHelper.RunOnUIThreadAndForget(async () =>
            {
                try
                {
                    using var _ = cancellationTokenSource;

                    await Task.Delay(dueTime, cancellationTokenSource.Token);

                    d.Disposable = action(this, state);
                }
                catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
                {
                    d.Disposable = DisposableAction.DoNothing;
                }
            });

            return d;
        }
    }
}