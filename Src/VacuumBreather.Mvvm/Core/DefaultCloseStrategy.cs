﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Used to gather the results from multiple child elements which may or may not prevent closing.</summary>
/// <typeparam name="T">The type of child element.</typeparam>
public class DefaultCloseStrategy<T> : ICloseStrategy<T>
{
    private readonly bool _closeConductedItemsWhenConductorCannotClose;

    /// <summary>Initializes a new instance of the <see cref="DefaultCloseStrategy{T}"/> class.</summary>
    /// <param name="closeConductedItemsWhenConductorCannotClose">
    ///     Indicates that even if all conducted items are not closable,
    ///     those that are should be closed. The default is <see langword="false"/>.
    /// </param>
    public DefaultCloseStrategy(bool closeConductedItemsWhenConductorCannotClose = false)
    {
        _closeConductedItemsWhenConductorCannotClose = closeConductedItemsWhenConductorCannotClose;
    }

    /// <inheritdoc/>
    public async ValueTask<ICloseResult<T>> ExecuteAsync(IEnumerable<T> toClose,
                                                         CancellationToken cancellationToken = default)
    {
        List<T> closeableChildren = new();
        var closeCanOccur = true;

        foreach (var child in toClose)
        {
            if (child is IGuardClose guarded)
            {
                var canClose = await guarded.CanCloseAsync(cancellationToken);

                if (canClose)
                {
                    closeableChildren.Add(child);
                }

                closeCanOccur = closeCanOccur && canClose;
            }
            else
            {
                closeableChildren.Add(child);
            }
        }

        if (!_closeConductedItemsWhenConductorCannotClose && !closeCanOccur)
        {
            closeableChildren.Clear();
        }

        return new CloseResult<T>(closeCanOccur, closeableChildren);
    }
}