using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Provides extension methods for the <see cref="ValueTask"/> type.</summary>
[PublicAPI]
public static class ValueTaskExtensions
{
    /// <summary>Uses the provided ticket to guard this task while it is running.</summary>
    /// <param name="task">The task to guard.</param>
    /// <param name="asyncGuard">The <see cref="AsyncGuard"/> to track the task with.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [SuppressMessage(category: "Style",
                     checkId: "VSTHRD200:Use \"Async\" suffix for async methods",
                     Justification = "This is an extension method for tasks. The async nature is already clear.")]
    public static async ValueTask Using(this ValueTask task, AsyncGuard asyncGuard)
    {
        using (asyncGuard.GetToken())
        {
            await task;
        }
    }

    /// <summary>Uses the provided ticket to guard this task while it is running.</summary>
    /// <typeparam name="T">The type of the task result.</typeparam>
    /// <param name="task">The task to guard.</param>
    /// <param name="asyncGuard">The <see cref="AsyncGuard"/> to track the task with.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the guarded task.</returns>
    [SuppressMessage(category: "Style",
                     checkId: "VSTHRD200:Use \"Async\" suffix for async methods",
                     Justification = "This is an extension method for tasks. The async nature is already clear.")]
    public static async ValueTask<T> Using<T>(this ValueTask<T> task, AsyncGuard asyncGuard)
    {
        using (asyncGuard.GetToken())
        {
            return await task;
        }
    }
}