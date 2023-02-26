using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>A conductor for dialogs.</summary>
public class DialogConductor : Screen, IDialogService
{
    private readonly ConductorCollectionOneActive<DialogScreen> _internalConductor = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="DialogConductor" /> class.
    /// </summary>
    public DialogConductor()
    {
        DisplayName = GetType().Name;
        _internalConductor.PropertyChanged += OnInternalConductorPropertyChanged;
    }

    /// <inheritdoc />
    public DialogScreen? ActiveItem => _internalConductor.ActiveItem;

    /// <inheritdoc />
    object? IHaveReadOnlyActiveItem.ActiveItem => ActiveItem;

    /// <summary>Shows the specified <see cref="DialogScreen" /> as a dialog.</summary>
    /// <param name="dialog">The dialog to show.</param>
    /// <param name="cancellationToken">
    ///     (Optional) A cancellation token that can be used by other objects
    ///     or threads to receive notice of cancellation.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask" /> that represents the asynchronous save operation.
    ///     The <see cref="ValueTask" /> result contains the dialog result.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Attempting to open a dialog with the same instance multiple times simultaneously.
    /// </exception>
    public virtual async ValueTask<bool?> ShowDialogAsync(DialogScreen dialog,
                                                          CancellationToken cancellationToken = default)
    {
        Guard.IsFalse(_internalConductor.Items.Contains(dialog),
                      nameof(dialog),
                      $"Attempting to open a {dialog.GetType().Name} dialog with the same instance multiple times simultaneously.");

        if (!_internalConductor.IsActive)
        {
            await _internalConductor.ActivateAsync(cancellationToken).ConfigureAwait(true);
        }

        await _internalConductor.ActivateItemAsync(dialog, cancellationToken).ConfigureAwait(true);

        return await dialog.GetDialogResultAsync().ConfigureAwait(true);
    }

    /// <inheritdoc />
    protected override async ValueTask OnDeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        if (close)
        {
            await _internalConductor.DeactivateAsync(close, cancellationToken).ConfigureAwait(true);
        }
    }

    private void OnInternalConductorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConductorCollectionOneActive<DialogScreen>.ActiveItem))
        {
            OnPropertyChanged(nameof(ActiveItem));
        }
    }
}