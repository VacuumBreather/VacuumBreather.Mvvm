using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>
///     Base application class that provides a basic initialization sequence.
/// </summary>
/// <seealso cref="System.Windows.Application" />
/// <remarks>
///     <para>This class must be overridden to provide application specific configuration.</para>
/// </remarks>
[SuppressMessage("Design",
                 "CA1001:Types that own disposable fields should be disposable",
                 Justification = "The fields in question are only ever instantiated in using blocks")]
[PublicAPI]
public abstract class MvvmApplication : Application
{
    private readonly IHost _host;

    private ILogger? _logger;

    private int _closingAttempts;
    private bool _isInitialized;

    private TaskCompletionSource? _onStartCompletion;
    private TaskCompletionSource? _onEnableCompletion;
    private TaskCompletionSource? _onDisableCompletion;
    private TaskCompletionSource? _shutdownCompletion;

    private SafeCancellationTokenSource? _onEnableCancellation;
    private SafeCancellationTokenSource? _onDisableCancellation;
    private IHostApplicationLifetime? _applicationLifetime;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MvvmApplication" /> class.
    /// </summary>
    protected MvvmApplication()
    {
        _host = Host.CreateDefaultBuilder()
                    .ConfigureHostBuilder(ConfigureHostBuilder)
                    .ConfigureServices(RegisterRequiredServices)
                    .Build();
    }

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="MvvmApplication" /> will react
    ///     to the main window activation and deactivation events and use them to activate and deactivate the main view-model.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if this <see cref="MvvmApplication" /> is reacting to the main
    ///     window activation events; otherwise,  <see langword="false" />.
    /// </value>
    protected bool IsReactingToWindowActivationEvents { get; set; }

    /// <summary>Gets the <see cref="ILogger" /> for this instance.</summary>
    protected ILogger Logger =>
        _logger ??= Services.GetService<ILoggerFactory>()?.CreateLogger(GetType()) ?? NullLogger.Instance;

    /// <summary>
    ///     Gets the application <see cref="IServiceProvider" />.
    /// </summary>
    /// <value>
    ///     The application <see cref="IServiceProvider" />.
    /// </value>
    protected IServiceProvider Services => _host.Services;

    /// <summary>
    ///     Gets or sets the timeout for stopping gracefully. Once expired the
    ///     host may terminate. The default value is 3 seconds.
    /// </summary>
    protected TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(3);

    private BindableObject? ShellViewModel { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return GetType().Name;
    }

    /// <summary>Shuts this <see cref="MvvmApplication" /> down.</summary>
    /// <remarks>
    ///     <para>
    ///         This is also called when the main window is closed but then it is not guaranteed to
    ///         finish if the shutdown process is a long running ValueTask.
    ///     </para>
    /// </remarks>
    /// <returns>A <see cref="ValueTask" /> that represents the asynchronous save operation.</returns>
    public async ValueTask ShutdownAsync()
    {
        if (!_isInitialized || (_shutdownCompletion != null))
        {
            return;
        }

        try
        {
            _isInitialized = false;

            Logger.LogInformation("Shutting down...");

            using var guard = TaskCompletion.CreateGuard(out _shutdownCompletion);

            await (_onStartCompletion?.Task ?? Task.CompletedTask).ConfigureAwait(true);
            await (_onDisableCompletion?.Task ?? Task.CompletedTask).ConfigureAwait(true);

            _onEnableCancellation?.Cancel();

            await OnShutdownAsync().ConfigureAwait(true);

            if (ShellViewModel is IDeactivate deactivate)
            {
                await deactivate.DeactivateAsync(true).ConfigureAwait(true);
            }

            using (_host)
            {
                await _host.StopAsync(ShutdownTimeout).ConfigureAwait(true);
            }
        }
        catch
        {
            Shutdown(-1);
        }
    }

    /// <summary>
    ///     Override to configure the host builder with custom logic.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder" />.</param>
    /// <returns>The configured <see cref="IHostBuilder" />.</returns>
    protected virtual IHostBuilder ConfigureHostBuilder(IHostBuilder hostBuilder)
    {
        return hostBuilder;
    }

    /// <summary>Override this to modify the configuration of the <see cref="ViewLocator" />.</summary>
    /// <param name="viewLocator">The <see cref="ViewLocator" /> to configure.</param>
    protected virtual void ConfigureViewLocator(ViewLocator viewLocator)
    {
    }

    /// <summary>
    ///     Override to handle unhandled exceptions.
    /// </summary>
    /// <param name="exception">
    ///     <para>The exception.</para>
    /// </param>
    /// <param name="source">
    ///     <para>
    ///         The source of the unhandled exception. This can be:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>AppDomain.CurrentDomain.UnhandledException</description>
    ///         </item>
    ///         <item>
    ///             <description>Application.Current.DispatcherUnhandledException</description>
    ///         </item>
    ///         <item>
    ///             <description>TaskScheduler.UnobservedTaskException</description>
    ///         </item>
    ///         <item>
    ///             <description>ThreadHelper.CleanUp</description>
    ///         </item>
    ///     </list>
    /// </param>
    protected virtual void HandleUnhandledException(Exception exception, string source)
    {
        Logger.LogCritical(exception, "Unhandled Exception - Source: {Source}", source);
    }

    /// <summary>
    ///     Triggered when the application host has completed a graceful shutdown.
    ///     The application will not exit until this method has completed.
    /// </summary>
    protected virtual void OnApplicationStopping()
    {
    }

    /// <summary>Override this to add custom logic on initialization.</summary>
    /// <returns>
    ///     <see langword="true" /> if the custom initialization logic was successful;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    protected virtual bool OnInitialize()
    {
        return true;
    }

    /// <summary>Override this to add custom logic on shutdown.</summary>
    /// <returns>A <see cref="ValueTask" /> that represents the asynchronous save operation.</returns>
    protected virtual ValueTask OnShutdownAsync()
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>Override this to add custom logic on startup.</summary>
    /// <returns>A <see cref="ValueTask" /> that represents the asynchronous save operation.</returns>
    protected virtual ValueTask OnStartupAsync()
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    ///     Resolves the <see cref="ResourceDictionary" /> containing the theme resources.
    /// </summary>
    /// <param name="services">The application <see cref="IServiceProvider" />.</param>
    /// <returns>
    ///     The <see cref="ResourceDictionary" /> containing the theme resources. Or <see langword="null" /> if no such
    ///     dictionary exists in the container.
    /// </returns>
    protected virtual ResourceDictionary? ResolveThemeResources(IServiceProvider services)
    {
        return null;
    }

    /// <inheritdoc />
    protected sealed override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        if (IsReactingToWindowActivationEvents)
        {
            OnEnableAsync().Forget();
        }
    }

    /// <inheritdoc />
    protected sealed override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);

        if (IsReactingToWindowActivationEvents)
        {
            OnDisableAsync().Forget();
        }
    }

    /// <inheritdoc />
    protected sealed override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _applicationLifetime = Services.GetRequiredService<IHostApplicationLifetime>();
        _applicationLifetime.ApplicationStarted.Register(OnApplicationStarted);
        _applicationLifetime.ApplicationStopping.Register(OnApplicationStopping);
        _applicationLifetime.ApplicationStopped.Register(OnApplicationStopped);

        ThreadHelper.CleanUpWith(_applicationLifetime,
                                 ShutdownTimeout,
                                 ex => HandleUnhandledException(ex,
                                                                $"{nameof(ThreadHelper)}.{nameof(ThreadHelper.CleanUp)}"));

        async ValueTask StartHost()
        {
            await _host.StartAsync().ConfigureAwait(true);
        }

        StartHost().Forget();
    }

    /// <summary>
    ///     Resolves the main view-model <see cref="BindableObject" /> (The data context of the main window).
    /// </summary>
    /// <param name="services">The application <see cref="IServiceProvider" />.</param>
    /// <returns>The main view-model <see cref="BindableObject" />.</returns>
    protected abstract BindableObject ResolveMainViewModel(IServiceProvider services);

    /// <summary>
    ///     Resolves the main application <see cref="Window" />.
    /// </summary>
    /// <param name="services">The application <see cref="IServiceProvider" />.</param>
    /// <returns>The main application <see cref="Window" />.</returns>
    protected abstract Window ResolveMainWindow(IServiceProvider services);

    private static void RegisterRequiredServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<ViewLocator>();
        services.AddSingleton<IDialogService, DialogConductor>();
    }

    private void AddServiceProviderToDictionary(IDictionary resourceDictionary)
    {
        resourceDictionary[nameof(IServiceProvider)] = _host.Services;
    }

    private void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        SetupExceptionHandling();

        Logger.LogInformation("Initializing...");

        var viewLocator = _host.Services.GetRequiredService<ViewLocator>();

        Logger.LogDebug("Configuring view locator");

        ConfigureViewLocator(viewLocator);

        bool wasOnInitializeSuccessful = OnInitialize();

        if (!wasOnInitializeSuccessful)
        {
            Logger.LogError("Initialization failed");

            return;
        }

        _isInitialized = true;

        Logger.LogInformation("Initialization completed");
    }

    private void OnApplicationStarted()
    {
        async ValueTask OnStarted()
        {
            await StartAsync().ConfigureAwait(true);
            await OnEnableAsync().ConfigureAwait(true);
        }

        OnStarted().Forget();
    }

    private void OnApplicationStopped()
    {
        Logger.LogInformation("Shutdown completed");
        Shutdown();
    }

    private async ValueTask OnDisableAsync()
    {
        using (_onDisableCancellation = new SafeCancellationTokenSource())
        {
            // Wait for startup completion.
            await (_onStartCompletion?.Task ?? Task.CompletedTask).ConfigureAwait(true);

            // Guard against multiple simultaneous executions.
            await (_onDisableCompletion?.Task ?? Task.CompletedTask).ConfigureAwait(true);
            using var guard = TaskCompletion.CreateGuard(out _onDisableCompletion);

            // Cancel activation and wait for potential synchronous steps to complete.
            _onEnableCancellation?.Cancel();
            await (_onStartCompletion?.Task ?? Task.CompletedTask).ConfigureAwait(true);
            await (_onEnableCompletion?.Task ?? Task.CompletedTask).ConfigureAwait(true);

            if (ShellViewModel is IDeactivate deactivate && !_onDisableCancellation.IsCancellationRequested)
            {
                await deactivate.DeactivateAsync(false, _onDisableCancellation.Token).ConfigureAwait(true);
            }
        }
    }

    private async ValueTask OnEnableAsync()
    {
        using (_onEnableCancellation = new SafeCancellationTokenSource())
        {
            // Wait for startup completion.
            await (_onStartCompletion?.Task ?? Task.CompletedTask).ConfigureAwait(true);

            // Guard against multiple simultaneous executions.
            await (_onEnableCompletion?.Task ?? Task.CompletedTask).ConfigureAwait(true);
            using var guard = TaskCompletion.CreateGuard(out _onEnableCompletion);

            // Cancel deactivation and wait for potential synchronous steps to complete.
            _onDisableCancellation?.Cancel();
            await (_onDisableCompletion?.Task ?? Task.CompletedTask).ConfigureAwait(true);

            if (ShellViewModel is IActivate activate && !_onEnableCancellation.IsCancellationRequested)
            {
                await activate.ActivateAsync(_onEnableCancellation.Token).ConfigureAwait(true);
            }
        }
    }

    private async ValueTask OnShellViewModelDeactivatedAsync(object sender,
                                                             DeactivationEventArgs e,
                                                             CancellationToken cancellationToken)
    {
        if (e.WasClosed)
        {
            await ShutdownAsync().ConfigureAwait(true);
        }
    }

    private void OnWindowClosing(object? _, CancelEventArgs args)
    {
        if (_closingAttempts > 0)
        {
            args.Cancel = true;
        }

        if (args.Cancel)
        {
            return;
        }

        args.Cancel = true;

        Interlocked.Increment(ref _closingAttempts);

        async ValueTask ClosingTask()
        {
            bool canClose = true;

            if (ShellViewModel is IGuardClose guardClose)
            {
                canClose = await guardClose.CanCloseAsync().ConfigureAwait(true);
            }

            if (canClose)
            {
                await ShutdownAsync().ConfigureAwait(true);
            }
            else
            {
                Interlocked.Decrement(ref _closingAttempts);
            }
        }

        ClosingTask().Forget();
    }

    private void SetupExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            HandleUnhandledException((Exception)e.ExceptionObject,
                                     $"{nameof(AppDomain)}.{nameof(AppDomain.CurrentDomain)}.{nameof(AppDomain.CurrentDomain.UnhandledException)}");
        };

        DispatcherUnhandledException += (_, e) =>
        {
            HandleUnhandledException(e.Exception,
                                     $"{nameof(Application)}.{nameof(Current)}.{nameof(DispatcherUnhandledException)}");

            e.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            HandleUnhandledException(e.Exception,
                                     $"{nameof(TaskScheduler)}.{nameof(TaskScheduler.UnobservedTaskException)}");

            e.SetObserved();
        };
    }

    private async ValueTask StartAsync()
    {
        // Guard against multiple executions.
        if (_onStartCompletion != null)
        {
            return;
        }

        using var guard = TaskCompletion.CreateGuard(out _onStartCompletion);

        Initialize();

        if (!_isInitialized)
        {
            Shutdown(-1);

            return;
        }

        Logger.LogInformation("Starting up...");

        DataTemplateManager.Logger = Services.GetService<ILoggerFactory>()?.CreateLogger(typeof(DataTemplateManager)) ??
                                     NullLogger.Instance;

        Logger.LogDebug("Registering data templates");

        DataTemplateManager.RegisterDataTemplates(Resources,
                                                  template => AddServiceProviderToDictionary(template.Resources));

        AddServiceProviderToDictionary(Resources);

        Logger.LogDebug("Merging resource dictionaries");

        var themeResources = ResolveThemeResources(Services);

        Resources.MergedDictionaries.Add(themeResources);

        var resources = Services.GetRequiredService<IEnumerable<ResourceDictionary>>()
                                .Where(dict => !ReferenceEquals(dict, themeResources));

        foreach (var resource in resources)
        {
            Resources.MergedDictionaries.Add(resource);
        }

        Logger.LogDebug("Resolving main view-model");

        ShellViewModel = ResolveMainViewModel(Services);

        if (ShellViewModel is IDeactivate deactivate)
        {
            deactivate.Deactivated += OnShellViewModelDeactivatedAsync;
        }

        Logger.LogDebug("Resolving main view");

        var mainView = ResolveMainWindow(Services);
        mainView.DataContext = ShellViewModel;
        mainView.Closing += OnWindowClosing;
        mainView.Show();

        await OnStartupAsync().ConfigureAwait(true);

        Logger.LogInformation("Startup complete");
    }
}