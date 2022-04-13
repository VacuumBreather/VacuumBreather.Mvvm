namespace VacuumBreather.Mvvm.Lifecycle
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using CommunityToolkit.Diagnostics;
    using JetBrains.Annotations;

    public partial class Conductor<T>
    {
        #region Nested Types

        /// <summary>An implementation of <see cref="IConductor" /> that holds on many items.</summary>
        public partial class Collection
        {
            #region Nested Types

            /// <summary>
            ///     An implementation of <see cref="IConductor" /> that holds on to many items which are all
            ///     activated.
            /// </summary>
            [PublicAPI]
            public class AllActive : ConductorBase<T>
            {
                #region Constants and Fields

                private readonly bool conductPublicItems;

                private readonly BindableCollection<T> items = new();

                #endregion

                #region Constructors and Destructors

                /// <summary>
                ///     Initializes a new instance of the
                ///     <see cref="VacuumBreather.Mvvm.Lifecycle.Conductor{T}.Collection.AllActive" /> class.
                /// </summary>
                /// <param name="conductPublicItems">
                ///     If set to <c>true</c> public items that are properties of this
                ///     class will be conducted.
                /// </param>
                public AllActive(bool conductPublicItems)
                    : this()
                {
                    this.conductPublicItems = conductPublicItems;
                }

                /// <summary>
                ///     Initializes a new instance of the
                ///     <see cref="VacuumBreather.Mvvm.Lifecycle.Conductor{T}.Collection.AllActive" /> class.
                /// </summary>
                public AllActive()
                {
                    this.items.AreChildrenOf(this);
                }

                #endregion

                #region Public Properties

                /// <summary>Gets the items that are currently being conducted.</summary>
                public IReadOnlyBindableCollection<T> Items => this.items;

                #endregion

                #region Public Methods

                /// <inheritdoc />
                public override async Task ActivateItemAsync(T item, CancellationToken cancellationToken = default)
                {
                    Guard.IsNotNull(item, nameof(item));

                    item = EnsureItem(item)!;

                    if (IsActive)
                    {
                        await ScreenExtensions.TryActivateAsync(item, cancellationToken);
                    }

                    OnActivationProcessed(item, true);
                }

                /// <inheritdoc />
                public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
                {
                    var closeResult = await CloseStrategy.ExecuteAsync(this.items.ToList(), cancellationToken);

                    if (!closeResult.CloseCanOccur && closeResult.Children.Any())
                    {
                        foreach (var deactivate in closeResult.Children.OfType<IDeactivate>())
                        {
                            await deactivate.DeactivateAsync(true, cancellationToken);
                        }

                        this.items.RemoveRange(closeResult.Children);
                    }

                    return closeResult.CloseCanOccur;
                }

                /// <inheritdoc />
                public override async Task DeactivateItemAsync(T item,
                                                               bool close,
                                                               CancellationToken cancellationToken = default)
                {
                    await this.DeactivateItemAsync(item, close, CloseItemCoreAsync, cancellationToken);
                }

                /// <inheritdoc />
                public override sealed IEnumerable<T> GetChildren()
                {
                    return this.items;
                }

                #endregion

                #region Protected Methods

                /// <inheritdoc />
                protected override T? EnsureItem(T? newItem)
                {
                    if (newItem is not null)
                    {
                        var index = this.items.IndexOf(newItem);

                        if (index == -1)
                        {
                            this.items.Add(newItem);
                        }
                        else
                        {
                            newItem = this.items[index];
                        }
                    }

                    return base.EnsureItem(newItem);
                }

                /// <inheritdoc />
                protected override async Task OnActivateAsync(CancellationToken cancellationToken)
                {
                    foreach (var activate in this.items.OfType<IActivate>())
                    {
                        await activate.ActivateAsync(cancellationToken);
                    }
                }

                /// <inheritdoc />
                protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
                {
                    foreach (var deactivate in this.items.OfType<IDeactivate>())
                    {
                        await deactivate.DeactivateAsync(close, cancellationToken);
                    }

                    if (close)
                    {
                        this.items.Clear();
                    }
                }

                /// <inheritdoc />
                protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
                {
                    if (this.conductPublicItems)
                    {
                        var publicItems = GetType()
                                          .GetTypeInfo()
                                          .DeclaredProperties
                                          .Where(
                                              propertyInfo =>
                                                  (propertyInfo.Name != nameof(Parent)) && typeof(T).GetTypeInfo()
                                                      .IsAssignableFrom(propertyInfo.PropertyType.GetTypeInfo()))
                                          .Select(propertyInfo => propertyInfo.GetValue(this, null))
                                          .Cast<T>()
                                          .ToList();

                        await Task.WhenAll(publicItems.Select(item => ActivateItemAsync(item, cancellationToken)));
                    }
                }

                #endregion

                #region Private Methods

                private async Task CloseItemCoreAsync(T item, CancellationToken cancellationToken = default)
                {
                    await ScreenExtensions.TryDeactivateAsync(item, true, cancellationToken);

                    this.items.Remove(item);
                }

                #endregion
            }

            #endregion
        }

        #endregion
    }
}