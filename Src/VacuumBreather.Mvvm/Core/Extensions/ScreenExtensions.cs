using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;

namespace VacuumBreather.Mvvm.Core;

/// <summary>
///     Provides extension methods for the <see cref="IScreen" /> and <see cref="IConductor" /> types.
/// </summary>
public static class ScreenExtensions
{
    /// <summary>Activates a child whenever the specified parent is activated.</summary>
    /// <param name="child">The child to activate.</param>
    /// <param name="parent">The parent whose activation triggers the child's activation.</param>
    public static void ActivateWith(this IActivate child, IActivate parent)
    {
        Guard.IsNotNull(parent);

        WeakReference? childReference = new(child);

        async ValueTask OnParentActivated(object sender, ActivationEventArgs _, CancellationToken cancellationToken)
        {
            if (childReference.Target is IActivate activate)
            {
                await activate.ActivateAsync(cancellationToken).ConfigureAwait(true);
            }
            else
            {
                ((IActivate)sender).Activated -= OnParentActivated;
            }
        }

        parent.Activated += OnParentActivated;
    }

    /// <summary>
    ///     Activates and Deactivates a child whenever the specified parent is Activated or
    ///     Deactivated.
    /// </summary>
    /// <typeparam name="TChild">The type of the conducted child.</typeparam>
    /// <typeparam name="TParent">The type of the conductor parent.</typeparam>
    /// <param name="child">The child to activate/deactivate.</param>
    /// <param name="parent">
    ///     The parent whose activation/deactivation triggers the child's
    ///     activation/deactivation.
    /// </param>
    public static void ConductWith<TChild, TParent>(this TChild child, TParent parent)
        where TChild : IActivate, IDeactivate
        where TParent : IActivate, IDeactivate
    {
        child.ActivateWith(parent);
        child.DeactivateWith(parent);
    }

    /// <summary>Deactivates a child whenever the specified parent is deactivated.</summary>
    /// <param name="child">The child to deactivate.</param>
    /// <param name="parent">The parent whose deactivation triggers the child's deactivation.</param>
    public static void DeactivateWith(this IDeactivate child, IDeactivate parent)
    {
        Guard.IsNotNull(parent);

        WeakReference? childReference = new(child);

        async ValueTask AsyncEventHandler(object sender, DeactivationEventArgs e, CancellationToken cancellationToken)
        {
            if (childReference.Target is IDeactivate deactivate)
            {
                await deactivate.DeactivateAsync(e.WasClosed, cancellationToken).ConfigureAwait(true);
            }
            else
            {
                ((IDeactivate)sender).Deactivated -= AsyncEventHandler;
            }
        }

        parent.Deactivated += AsyncEventHandler;
    }

    /// <summary>Activates the item if it implements <see cref="IActivate" />, otherwise does nothing.</summary>
    /// <param name="potentialActivate">The potential activate.</param>
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    public static async ValueTask TryActivateAsync(object? potentialActivate,
                                                   CancellationToken cancellationToken = default)
    {
        if (potentialActivate is IActivate activator)
        {
            await activator.ActivateAsync(cancellationToken).ConfigureAwait(true);
        }
    }

    /// <summary>Deactivates the item if it implements <see cref="IDeactivate" />, otherwise does nothing.</summary>
    /// <param name="potentialDeactivate">The potential deactivate.</param>
    /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    public static async ValueTask TryDeactivateAsync(object? potentialDeactivate,
                                                     bool close,
                                                     CancellationToken cancellationToken = default)
    {
        if (potentialDeactivate is IDeactivate deactivate)
        {
            await deactivate.DeactivateAsync(close, cancellationToken).ConfigureAwait(true);
        }
    }
}