using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Provides extension methods for the <see cref="IAsyncOperation"/> type.</summary>
[PublicAPI]
public static class AsyncOperationExtensions
{
    /// <summary>Assigns the <see cref="IAsyncOperation"/> to an out variable for external use.</summary>
    /// <typeparam name="TAsyncOperation">The type of the asynchronous operation.</typeparam>
    /// <param name="operation">The asynchronous operation.</param>
    /// <param name="asyncOperation">The out variable to assign the <see cref="IAsyncOperation"/> to.</param>
    /// <returns>The same instance of the <see cref="IAsyncOperation"/> for chaining.</returns>
    public static TAsyncOperation Assign<TAsyncOperation>(this TAsyncOperation operation,
                                                          out IAsyncOperation asyncOperation)
        where TAsyncOperation : class, IAsyncOperation
    {
        asyncOperation = operation;

        return operation;
    }
}