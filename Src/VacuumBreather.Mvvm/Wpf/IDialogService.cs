using System.Threading;
using System.Threading.Tasks;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>Interface for conductors of dialogs.</summary>
public interface IDialogService : IHaveReadOnlyActiveItem<DialogScreen>, IScreen
{
    /// <summary>Shows the specified <see cref="DialogScreen" /> as a dialog.</summary>
    /// <param name="dialog">The dialog to show.</param>
    /// <param name="cancellationToken">
    ///     (Optional) A cancellation token that can be used by other objects
    ///     or threads to receive notice of cancellation.
    /// </param>
    /// <returns>
    ///     A ValueTask that represents the asynchronous save operation. The ValueTask result contains the
    ///     dialog result.
    /// </returns>
    ValueTask<bool?> ShowDialogAsync(DialogScreen dialog, CancellationToken cancellationToken = default);
}