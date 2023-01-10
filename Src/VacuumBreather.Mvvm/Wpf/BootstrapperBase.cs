// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Logging;
using VacuumBreather.Mvvm.Lifecycle;

// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>Inherit from this class to configure and run the framework.</summary>
    public abstract class BootstrapperBase<TMainContent> : IServiceProvider
        where TMainContent : Screen
    {
        private bool isInitialized;

        /// <summary>Initializes a new instance of the <see cref="BootstrapperBase{TMainContent}" /> class.</summary>
        protected BootstrapperBase()
        {
            Application.Current.Startup += async (_, _) =>
            {
                await OnEnableAsync();
                await StartAsync();
            };
            Application.Current.Activated += async (_, _) =>
            {
                await OnEnableAsync();
            };
            Application.Current.Deactivated += async (_, _) =>
            {
                await OnDisableAsync();
            };
        }

        /// <summary>Gets or sets the <see cref="Microsoft.Extensions.Logging.ILogger" /> for this instance.</summary>
        protected ILogger Logger => LogManager.FrameworkLogger;

        private Container? IoCContainer { get; set; }

        private IWindowManager? WindowManager { get; set; }

        private ViewLocator? ViewLocator { get; set; }

        /// <inheritdoc />
        /// <remarks>This method must not throw exceptions.</remarks>
        public virtual object? GetService(Type serviceType)
        {
            try
            {
                return IoCContainer?.GetInstance(serviceType);
            }
            catch (Exception serviceLookupException)
            {
                Logger.LogError(serviceLookupException, "Failed to find service {ServiceType}", serviceType);

                return null;
            }
        }

        /// <summary>Gets the service object of the specified type.</summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <returns>
        ///     A service object of type serviceType.<br />-or-<br /><c>null</c> if there is no service
        ///     object of type serviceType.
        /// </returns>
        public TService? GetService<TService>()
        {
            return (TService?)GetService(typeof(TService));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return GetType().Name;
        }

        /// <summary>Shuts the UI handled by this <see cref="BootstrapperBase{T}" /> down.</summary>
        /// <remarks>
        ///     This is also called when the bootstrapper is destroyed but then it is not guaranteed to
        ///     finish if the shutdown process is a long running ValueTask.
        /// </remarks>
        public async ValueTask ShutdownAsync()
        {
            if (!this.isInitialized)
            {
                return;
            }

            try
            {
                Logger.LogInformation("Shutting down...");

                await OnShutdown();

                if (WindowManager is IDeactivate deactivate)
                {
                    await deactivate.DeactivateAsync(true);
                }

                Logger.LogInformation("Shutdown complete");
            }
            finally
            {
                this.isInitialized = false;

                Application.Current.Shutdown();
            }
        }

        /// <summary>Override to configure your dependency injection container.</summary>
        /// <remarks>
        ///     If you are configuring your own DI container you also need to override
        ///     <see cref="GetService" /> to let the framework use it. <br /> The following types need to be
        ///     registered here at a minimum:
        ///     <list type="bullet">
        ///         <item>An <see cref="AssemblySource" /> instance, using singleton lifetime in this scope.</item>
        ///         <item>A <see cref="ViewLocator" />, using singleton lifetime in this scope</item>
        ///         <item>
        ///             An <see cref="IServiceProvider" /> implementation, using singleton lifetime in this
        ///             scope. This is usually the bootstrapper itself.
        ///         </item>
        ///         <item>
        ///             An <see cref="IWindowManager" /> implementation, using singleton lifetime in this
        ///             scope
        ///         </item>
        ///         <item>
        ///             Optionally an <see cref="ILogger" />, using singleton lifetime in this scope, if you
        ///             want to provide your own to the framework
        ///         </item>
        ///         <item>All relevant views, view-models and services</item>
        ///     </list>
        /// </remarks>
        protected virtual void ConfigureIoCContainer()
        {
            IoCContainer = new Container();

            AssemblySource assemblySource = new();
            assemblySource.Add(GetType().Assembly);

            IoCContainer.RegisterInstance(assemblySource);
            IoCContainer.RegisterSingleton<ViewLocator, ViewLocator>();
            IoCContainer.RegisterSingleton<IWindowManager, ShellViewModel>();

            DebugLogger logger = new(LogManager.FrameworkCategoryName);

            IoCContainer.RegisterInstance(typeof(ILogger), logger);

            foreach (Type viewType in assemblySource.ViewTypes)
            {
                IoCContainer.RegisterPerRequest(viewType, viewType);
            }

            foreach (Type type in assemblySource.ViewModelTypes)
            {
                IoCContainer.RegisterPerRequest(type, type);

                foreach (Type @interface in type.GetInterfaces())
                {
                    IoCContainer.RegisterPerRequest(@interface, type);
                }
            }

            LogManager.FrameworkLogger = logger;
        }

        /// <summary>Override this to modify the configuration of the <see cref="Mvvm.Wpf.ViewLocator" />.</summary>
        /// <param name="viewLocator">The <see cref="Mvvm.Wpf.ViewLocator" /> to configure.</param>
        protected virtual void ConfigureViewLocator(ViewLocator viewLocator)
        {
        }

        /// <summary>Override this to add custom logic on initialization.</summary>
        /// <returns><c>true</c> if the custom initialization logic was successful; otherwise, <c>false</c>.</returns>
        protected virtual bool OnInitialize()
        {
            return true;
        }

        /// <summary>Override this to add custom logic on shutdown.</summary>
        protected virtual ValueTask OnShutdown()
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>Override this to add custom logic on startup.</summary>
        protected virtual ValueTask OnStartup()
        {
            return ValueTask.CompletedTask;
        }

        private async void OnWindowClosing(object? _, CancelEventArgs args)
        {
            if (args.Cancel)
            {
                return;
            }

            bool canClose = true;

            if (WindowManager is IGuardClose guardClose)
            {
                canClose = await guardClose.CanCloseAsync();
            }

            args.Cancel = true;

            if (canClose)
            {
                await ShutdownAsync();
            }
        }

        private void AddServiceProviderResource(ResourceDictionary resourceDictionary)
        {
            resourceDictionary[nameof(IServiceProvider)] = this;
        }

        private void Initialize()
        {
            if (this.isInitialized)
            {
                return;
            }

            ConfigureIoCContainer();

            Logger.LogInformation("Initializing...");

            ViewLocator = GetService<ViewLocator>();

            if (ViewLocator is null)
            {
                Logger.LogError(
                    "View locator not found - please register {ViewLocator} instance in DI container",
                    nameof(ViewLocator));
                Logger.LogError("Initialization failed");

                return;
            }

            Logger.LogInformation("Configuring type mappings");

            ViewLocator.ConfigureTypeMappings(new TypeMappingConfiguration());
            ConfigureViewLocator(ViewLocator);

            Logger.LogInformation("Resolving window manager");
            WindowManager = GetService<IWindowManager>();

            if (WindowManager is null)
            {
                Logger.LogError(
                    "Window manager not found - please register {IWindowManager} implementation in DI container",
                    nameof(IWindowManager));
                Logger.LogError("Initialization failed");

                return;
            }

            bool wasOnInitializeSuccessful = OnInitialize();

            if (!wasOnInitializeSuccessful)
            {
                Logger.LogError("Initialization failed");

                return;
            }

            this.isInitialized = true;
            Logger.LogInformation("Initialization complete");
        }

        private async ValueTask OnDisableAsync(CancellationToken cancellationToken = default)
        {
            if (WindowManager is IDeactivate deactivate)
            {
                await deactivate.DeactivateAsync(false, cancellationToken);
            }
        }

        private async ValueTask OnEnableAsync(CancellationToken cancellationToken = default)
        {
            if (WindowManager is IActivate activate)
            {
                await activate.ActivateAsync(cancellationToken);
            }
        }

        private async ValueTask StartAsync()
        {
            Initialize();

            if (!this.isInitialized)
            {
                return;
            }

            Logger.LogInformation("Starting...");

            ResourceDictionary dictionary = Application.Current.Resources;

            Logger.LogInformation("Registering data templates");

            DataTemplateManager.RegisterDataTemplates(
                ViewLocator!,
                dictionary,
                template => AddServiceProviderResource(template.Resources));

            AddServiceProviderResource(dictionary);

            ShellView shellView = new() { DataContext = WindowManager };
            shellView.Show();
            shellView.Closing += OnWindowClosing;

            await OnStartup();

            TMainContent? mainContent = GetService<TMainContent>();

            Logger.LogInformation("Showing main content: {MainContent}", mainContent);

            if (mainContent is not null)
            {
                await WindowManager!.ShowMainContentAsync(mainContent);
            }

            if (WindowManager is IActivate activate)
            {
                await activate.ActivateAsync();
            }

            Logger.LogInformation("Start complete");
        }
    }
}