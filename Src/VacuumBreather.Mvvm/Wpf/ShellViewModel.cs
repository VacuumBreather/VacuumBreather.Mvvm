// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using VacuumBreather.Mvvm.Lifecycle;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>The root view model of the UI.</summary>
    public class ShellViewModel : ConductorCollectionAllActive<IConductor>, IWindowManager
    {
        /// <summary>Initializes a new instance of the <see cref="ShellViewModel" /> class.</summary>
        public ShellViewModel()
            : base(true)
        {
            DisplayName = "RootConductor";
            MainContentConductor.DisplayName = "MainContentConductor";
            DialogConductor.DisplayName = "DialogConductor";
        }

        /// <summary>Gets the conductor which hosts any dialog content.</summary>
        public DialogConductor DialogConductor { get; } = new();

        /// <summary>Gets the conductor which hosts the main content.</summary>
        public Conductor<Screen> MainContentConductor { get; } = new();

        /// <inheritdoc />
        public async ValueTask<bool?> ShowDialogAsync(DialogScreen rootModel,
            CancellationToken cancellationToken = default)
        {
            return await DialogConductor.ShowDialogAsync(rootModel, cancellationToken);
        }

        /// <inheritdoc />
        public async ValueTask ShowMainContentAsync(Screen rootModel, CancellationToken cancellationToken = default)
        {
            await MainContentConductor.ActivateItemAsync(rootModel, cancellationToken);
        }
    }
}