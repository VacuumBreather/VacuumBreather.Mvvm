using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Win32;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf.Dialogs;

/// <summary>A conductor handling dialogs.</summary>
[PublicAPI]
public class DialogConductor : ConductorCollectionOneActive<DialogScreen>, IDialogService
{
    /// <summary>Initializes a new instance of the <see cref="DialogConductor"/> class.</summary>
    public DialogConductor()
    {
        DisplayName = GetType().Name;
    }

    /// <inheritdoc/>
    object? IHaveReadOnlyActiveItem.ActiveItem => ActiveItem;

    /// <inheritdoc/>
    public virtual ValueTask<string?> ShowOpenFileDialogAsync(FileDialogOptions options,
                                                              CancellationToken cancellationToken = default)
    {
        var openFileDialog = new OpenFileDialog
                             {
                                 Filter = options.Filter,
                                 InitialDirectory = options.InitialDirectory,
                                 RestoreDirectory = options.RestoreDirectory,
                             };

        return ThreadHelper.RunOnUIThreadAsync(() =>
                                               {
                                                   if (openFileDialog.ShowDialog() == true)
                                                   {
                                                       return openFileDialog.FileName;
                                                   }

                                                   return null;
                                               },
                                               cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string?> ShowSaveFileDialogAsync(FileDialogOptions options,
                                                              CancellationToken cancellationToken = default)
    {
        var openFileDialog = new SaveFileDialog
                             {
                                 Filter = options.Filter,
                                 InitialDirectory = options.InitialDirectory,
                                 RestoreDirectory = options.RestoreDirectory,
                             };

        return ThreadHelper.RunOnUIThreadAsync(() =>
                                               {
                                                   if (openFileDialog.ShowDialog() == true)
                                                   {
                                                       return openFileDialog.FileName;
                                                   }

                                                   return null;
                                               },
                                               cancellationToken: cancellationToken);
    }

    /// <summary>Shows the specified <see cref="DialogScreen"/> as a dialog.</summary>
    /// <param name="dialog">The dialog to show.</param>
    /// <param name="cancellationToken">
    ///     (Optional) A cancellation token that can be used by other objects or threads to receive
    ///     notice of cancellation.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask"/> that represents the asynchronous save operation. The <see cref="ValueTask"/> result
    ///     contains the dialog result.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Attempting to open a dialog with the same instance multiple times
    ///     simultaneously.
    /// </exception>
    public async ValueTask<DialogResult> ShowDialogAsync(DialogScreen dialog,
                                                         CancellationToken cancellationToken = default)
    {
        Guard.IsFalse(Items.Contains(dialog),
                      nameof(dialog),
                      $"Attempting to open a {dialog.GetType().Name} dialog with the same instance multiple times simultaneously.");

        await ActivateItemAsync(dialog, cancellationToken);

        return await dialog.GetDialogResultAsync();
    }
}