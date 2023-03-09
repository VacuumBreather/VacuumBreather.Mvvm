using System.Threading;
using System.Threading.Tasks;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>Interface for a service for opening various dialogs and awaiting their results.</summary>
public interface IDialogService : IHaveReadOnlyActiveItem<DialogScreen>, IScreen
{
    /// <summary>Shows the specified <see cref="DialogScreen"/> as a dialog.</summary>
    /// <param name="dialog">The dialog to show.</param>
    /// <param name="cancellationToken">
    ///     (Optional) A cancellation token that can be used by other objects or threads to receive
    ///     notice of cancellation.
    /// </param>
    /// <returns>A ValueTask that represents the asynchronous save operation. The ValueTask result contains the dialog result.</returns>
    ValueTask<bool?> ShowDialogAsync(DialogScreen dialog, CancellationToken cancellationToken = default);

    /// <summary>Shows a dialog to open a file and returns the user selected filename.</summary>
    /// <param name="options">The options configuring the file dialog.</param>
    /// <param name="cancellationToken">
    ///     (Optional) A cancellation token that can be used by other objects or threads to receive
    ///     notice of cancellation.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask"/> that represents the asynchronous save operation. The <see cref="ValueTask"/> result
    ///     contains the selected filename, or <see langword="null"/> if the dialog was cancelled.
    /// </returns>
    ValueTask<string?> ShowOpenFileDialogAsync(FileDialogOptions options,
                                               CancellationToken cancellationToken = default);

    /// <summary>Shows a dialog to save a file and returns the user selected filename.</summary>
    /// <param name="options">The options configuring the file dialog.</param>
    /// <param name="cancellationToken">
    ///     (Optional) A cancellation token that can be used by other objects or threads to receive
    ///     notice of cancellation.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask"/> that represents the asynchronous save operation. The <see cref="ValueTask"/> result
    ///     contains the selected filename, or <see langword="null"/> if the dialog was cancelled.
    /// </returns>
    ValueTask<string?> ShowSaveFileDialogAsync(FileDialogOptions options,
                                               CancellationToken cancellationToken = default);
}