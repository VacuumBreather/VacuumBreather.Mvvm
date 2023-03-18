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
using VacuumBreather.Mvvm.Wpf.Dialogs;
using VacuumBreather.Mvvm.Wpf.Notifications;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>Base application class that provides a basic initialization sequence.</summary>
/// <seealso cref="System.Windows.Application"/>
/// <remarks>
///     <para>This class must be overridden to provide application specific configuration.</para>
/// </remarks>
[SuppressMessage(category: "Design",
                 checkId: "CA1001:Types that own disposable fields should be disposable",
                 Justification =
                     "The fields in question are only ever instantiated in using blocks. The host is also cleaned up.")]
[SuppressMessage(category: "IDisposableAnalyzers.Correctness",
                 checkId: "IDISP006:Implement IDisposable",
                 Justification =
                     "The fields in question are only ever instantiated in using blocks. The host is also cleaned up.")]
[PublicAPI]
public abstract class MvvmApplication : Application
{
    private readonly IHost _host;

    private ILogger? _logger;

    private int _closingAttempts;
    private bool _isInitialized;

    private IAsyncOperation? _startOperation;
    private IAsyncOperation? _enableOperation;
    private IAsyncOperation? _disableOperation;
    private IAsyncOperation? _shutdownOperation;

    /// <summary>Initializes a new instance of the <see cref="MvvmApplication"/> class.</summary>
    protected MvvmApplication()
    {
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _host = Host.CreateDefaultBuilder(Environment.GetCommandLineArgs())
                    .ConfigureHostBuilder(ConfigureHostBuilder)
                    .ConfigureServices(RegisterRequiredServices)
                    .Build();
    }

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="MvvmApplication"/> will react to the main window
    ///     activation and deactivation events and use them to activate and deactivate the main view-model.
    /// </summary>
    /// <value>
    ///     <see langword="true"/> if this <see cref="MvvmApplication"/> is reacting to the main window activation events;
    ///     otherwise,  <see langword="false"/>.
    /// </value>
    protected bool IsReactingToWindowActivationEvents { get; set; }

    /// <summary>Gets the <see cref="ILogger"/> for this instance.</summary>
    protected ILogger Logger =>
        _logger ??= Services.GetService<ILoggerFactory>()?.CreateLogger(GetType()) ?? NullLogger.Instance;

    /// <summary>Gets the application <see cref="IServiceProvider"/>.</summary>
    /// <value>The application <see cref="IServiceProvider"/>.</value>
    protected IServiceProvider Services => _host.Services;

    /// <summary>
    ///     Gets or sets the timeout for stopping gracefully. Once expired the host may terminate. The default value is 3
    ///     seconds.
    /// </summary>
    protected TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(value: 3);

    private BindableObject? ShellViewModel { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return GetType().Name;
    }

    /// <summary>Shuts this <see cref="MvvmApplication"/> down.</summary>
    /// <remarks>
    ///     <para>
    ///         This is also called when the main window is closed but then it is not guaranteed to finish if the shutdown
    ///         process is a long running ValueTask.
    ///     </para>
    /// </remarks>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous save operation.</returns>
    public async ValueTask ShutdownAsync()
    {
        // Guard against multiple executions.
        if (!_isInitialized || _shutdownOperation is not null)
        {
            return;
        }

        try
        {
            _isInitialized = false;

            Logger.LogInformation(message: "Shutting down...");

            using var operation = AsyncHelper.CreateAsyncOperation().Assign(out _shutdownOperation);

            await AsyncHelper.AwaitCompletionAsync(_startOperation);
            await AsyncHelper.AwaitCompletionAsync(_disableOperation);

            _enableOperation?.Cancel();

            await OnShutdownAsync();

            if (ShellViewModel is IDeactivate deactivate)
            {
                await deactivate.DeactivateAsync(close: true);
            }

            using (_host)
            {
                await _host.StopAsync(ShutdownTimeout);
            }
        }
        catch
        {
            Shutdown(exitCode: -1);

            throw;
        }
    }

    /// <summary>Override to configure the host builder with custom logic.</summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/>.</param>
    /// <returns>The configured <see cref="IHostBuilder"/>.</returns>
    protected virtual IHostBuilder ConfigureHostBuilder(IHostBuilder hostBuilder)
    {
        return hostBuilder;
    }

    /// <summary>Override this to modify the configuration of the <see cref="ViewLocator"/>.</summary>
    /// <param name="viewLocator">The <see cref="ViewLocator"/> to configure.</param>
    protected virtual void ConfigureViewLocator(ViewLocator viewLocator)
    {
    }

    /// <summary>Override to handle unhandled exceptions.</summary>
    /// <param name="exception">
    ///     <para>The exception.</para>
    /// </param>
    /// <param name="source">
    ///     <para>The source of the unhandled exception. This can be:</para>
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
        Logger.LogCritical(exception, message: "Unhandled Exception - Source: {Source}", source);
    }

    /// <summary>
    ///     Triggered when the application host has completed a graceful shutdown. The application will not exit until
    ///     this method has completed.
    /// </summary>
    protected virtual void OnApplicationStopping()
    {
    }

    /// <summary>Override this to add custom logic on initialization.</summary>
    /// <returns><see langword="true"/> if the custom initialization logic was successful; otherwise, <see langword="false"/> .</returns>
    protected virtual bool OnInitialize()
    {
        return true;
    }

    /// <summary>Override this to add custom logic on shutdown.</summary>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous save operation.</returns>
    protected virtual ValueTask OnShutdownAsync()
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>Override this to add custom logic on startup.</summary>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous save operation.</returns>
    protected virtual ValueTask OnStartupAsync()
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>Resolves the <see cref="ResourceDictionary"/> containing the theme resources.</summary>
    /// <param name="services">The application <see cref="IServiceProvider"/>.</param>
    /// <returns>
    ///     The <see cref="ResourceDictionary"/> containing the theme resources. Or <see langword="null"/> if no such
    ///     dictionary exists in the container.
    /// </returns>
    protected virtual ResourceDictionary ResolveThemeResources(IServiceProvider services)
    {
        return new ResourceDictionary();
    }

    /// <inheritdoc/>
    protected sealed override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        if (IsReactingToWindowActivationEvents)
        {
            OnEnableAsync().Forget();
        }
    }

    /// <inheritdoc/>
    protected sealed override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);

        if (IsReactingToWindowActivationEvents)
        {
            OnDisableAsync().Forget();
        }
    }

    /// <inheritdoc/>
    protected sealed override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var applicationLifetime = Services.GetRequiredService<IHostApplicationLifetime>();
        applicationLifetime.ApplicationStarted.Register(OnApplicationStarted);
        applicationLifetime.ApplicationStopping.Register(OnApplicationStopping);
        applicationLifetime.ApplicationStopped.Register(OnApplicationStopped);

        ThreadHelper.Initialize(Dispatcher,
                                applicationLifetime,
                                ShutdownTimeout,
                                ex => HandleUnhandledException(ex,
                                                               $"{nameof(ThreadHelper)}.{nameof(ThreadHelper.CleanUp)}"));

        async ValueTask StartHost() => await _host.StartAsync();

        StartHost().Forget();
    }

    /// <summary>Resolves the main view-model <see cref="BindableObject"/> (The data context of the main window).</summary>
    /// <param name="services">The application <see cref="IServiceProvider"/>.</param>
    /// <returns>The main view-model <see cref="BindableObject"/>.</returns>
    protected abstract BindableObject ResolveMainViewModel(IServiceProvider services);

    /// <summary>Resolves the main application <see cref="Window"/>.</summary>
    /// <param name="services">The application <see cref="IServiceProvider"/>.</param>
    /// <returns>The main application <see cref="Window"/>.</returns>
    protected abstract Window ResolveMainWindow(IServiceProvider services);

    private static void RegisterRequiredServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<ViewLocator>();
        services.AddSingleton<INotificationService, NotificationConductor>();
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

        Logger.LogInformation(message: "Initializing...");

        var viewLocator = _host.Services.GetRequiredService<ViewLocator>();

        Logger.LogDebug(message: "Configuring view locator");

        ConfigureViewLocator(viewLocator);

        var wasOnInitializeSuccessful = OnInitialize();

        if (!wasOnInitializeSuccessful)
        {
            Logger.LogError(message: "Initialization failed");

            return;
        }

        _isInitialized = true;

        Logger.LogInformation(message: "Initialization completed");
    }

    private void InitializeResources()
    {
        DataTemplateManager.Logger = Services.GetService<ILoggerFactory>()?.CreateLogger(typeof(DataTemplateManager)) ??
                                     NullLogger.Instance;

        Logger.LogDebug(message: "Registering data templates");

        DataTemplateManager.RegisterDataTemplates(Resources,
                                                  template => AddServiceProviderToDictionary(template.Resources));

        AddServiceProviderToDictionary(Resources);

        Logger.LogDebug(message: "Merging resource dictionaries");

        var themeResources = ResolveThemeResources(Services);

        Resources.MergedDictionaries.Add(themeResources);

        var resources = Services.GetRequiredService<IEnumerable<ResourceDictionary>>()
                                .Where(dict => !ReferenceEquals(dict, themeResources));

        foreach (var resource in resources)
        {
            Resources.MergedDictionaries.Add(resource);
        }
    }

    private void OnApplicationStarted()
    {
        async ValueTask OnStarted()
        {
            await StartAsync();
            await OnEnableAsync();
        }

        OnStarted().Forget();
    }

    private void OnApplicationStopped()
    {
        Logger.LogInformation(message: "Shutdown completed");
        Shutdown();
    }

    private async ValueTask OnDisableAsync()
    {
        // Wait for startup completion.
        await AsyncHelper.AwaitCompletionAsync(_startOperation);

        // Guard against multiple simultaneous executions.
        await AsyncHelper.AwaitCompletionAsync(_disableOperation);

        using var operation = AsyncHelper.CreateAsyncOperation().Assign(out _disableOperation);

        // Cancel activation and wait for potential synchronous steps to complete.
        _enableOperation?.Cancel();
        await AsyncHelper.AwaitCompletionAsync(_enableOperation);

        if (ShellViewModel is IDeactivate deactivate && !_disableOperation.IsCancellationRequested)
        {
            await deactivate.DeactivateAsync(close: false, _disableOperation.Token);
        }
    }

    private async ValueTask OnEnableAsync()
    {
        // Wait for startup completion.
        await AsyncHelper.AwaitCompletionAsync(_startOperation);

        // Guard against multiple simultaneous executions.
        await AsyncHelper.AwaitCompletionAsync(_enableOperation);

        using var operation = AsyncHelper.CreateAsyncOperation().Assign(out _enableOperation);

        // Cancel deactivation and wait for potential synchronous steps to complete.
        _disableOperation?.Cancel();
        await AsyncHelper.AwaitCompletionAsync(_disableOperation);

        if (ShellViewModel is IActivate activate && !_enableOperation.IsCancellationRequested)
        {
            await activate.ActivateAsync(_enableOperation.Token);
        }
    }

    private async ValueTask OnShellViewModelDeactivatedAsync(object sender,
                                                             DeactivationEventArgs e,
                                                             CancellationToken cancellationToken)
    {
        if (e.WasClosed)
        {
            await ShutdownAsync();
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
            var canClose = true;

            if (ShellViewModel is IGuardClose guardClose)
            {
                canClose = await guardClose.CanCloseAsync();
            }

            if (canClose)
            {
                await ShutdownAsync();
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
        if (_startOperation is not null)
        {
            return;
        }

        using var operation = AsyncHelper.CreateAsyncOperation().Assign(out _startOperation);

        Initialize();

        if (!_isInitialized)
        {
            Shutdown(exitCode: -1);

            return;
        }

        Logger.LogInformation(message: "Starting up...");

        InitializeResources();

        Logger.LogDebug(message: "Resolving main view-model");

        ShellViewModel = ResolveMainViewModel(Services);

        if (ShellViewModel is IDeactivate deactivate)
        {
            deactivate.Deactivated += OnShellViewModelDeactivatedAsync;
        }

        Logger.LogDebug(message: "Resolving main view");

        await Services.GetRequiredService<INotificationService>().ActivateAsync(operation.Token);
        await Services.GetRequiredService<IDialogService>().ActivateAsync(operation.Token);

        var mainView = ResolveMainWindow(Services);
        mainView.DataContext = ShellViewModel;
        mainView.Closing += OnWindowClosing;
        mainView.Show();

        await OnStartupAsync();

        Logger.LogInformation(message: "Startup complete");
    }
}