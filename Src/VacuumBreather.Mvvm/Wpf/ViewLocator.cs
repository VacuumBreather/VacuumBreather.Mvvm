using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>Responsible for mapping view-model types to their corresponding view types.</summary>
[PublicAPI]
public class ViewLocator
{
    /// <summary>A dictionary that contains all the registered factories for the views.</summary>
    private readonly Dictionary<string, Func<FrameworkElement?>> _factories = new(StringComparer.Ordinal);

    /// <summary>A dictionary that contains all the registered ViewModel types for the views.</summary>
    private readonly Dictionary<string, Type> _typeFactories = new(StringComparer.Ordinal);

    /// <summary>The default view factory which provides the view type as a parameter.</summary>
    private Func<Type, FrameworkElement?> _defaultViewFactory;

    /// <summary>
    ///     Default view type to view-model type resolver, assumes the view-model is in same assembly as the view type,
    ///     but in the "ViewModels" namespace.
    /// </summary>
    private Func<Type, Type?> _defaultViewModelTypeToViewTypeResolver = viewModelType =>
    {
        var viewModelName = viewModelType.FullName;

        viewModelName = viewModelName?.Replace(oldValue: ".ViewModels.",
                                               newValue: ".Views.",
                                               StringComparison.InvariantCulture);

        var viewAssemblyName = viewModelType.GetTypeInfo().Assembly.FullName;

        viewModelName =
            viewModelName?.Replace(oldValue: "ViewModel", newValue: "View", StringComparison.InvariantCulture);

        var viewName = $"{viewModelName}, {viewAssemblyName}";

        return Type.GetType(viewName);
    };

    /// <summary>Initializes a new instance of the <see cref="ViewLocator"/> class.</summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> used to resolve views.</param>
    public ViewLocator(IServiceProvider serviceProvider)
    {
        _defaultViewFactory = viewType =>
        {
            if (viewType.IsInterface ||
                viewType.IsAbstract ||
                !viewType.IsDerivedFromOrImplements(typeof(FrameworkElement)))
            {
                return null;
            }

            if (serviceProvider.GetService(viewType) is FrameworkElement view)
            {
                return view;
            }

            try
            {
                return (FrameworkElement?)Activator.CreateInstance(viewType);
            }
            catch (Exception exception) when (exception is ArgumentException or
                                                           NotSupportedException or
                                                           TargetInvocationException or
                                                           MethodAccessException or
                                                           MemberAccessException or
                                                           InvalidComObjectException or
                                                           MissingMethodException or
                                                           COMException or
                                                           TypeLoadException)
            {
                return null;
            }
        };
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the <see cref="ViewLocator"/> should call InitializeComponent on a
    ///     located view via reflection. This makes a code behind file with just a constructor unnecessary. Since
    ///     InitializeComponent is guarded against multiple invocations this is safe, aside from the cost of the reflection
    ///     call. Subsequent calls will be redundant.
    /// </summary>
    /// <value>
    ///     <see langword="true"/> if the <see cref="ViewLocator"/> should call InitializeComponent on a located view via
    ///     reflection; otherwise, <see langword="false"/>.
    /// </value>
    public bool CallInitializeComponent { get; set; }

    /// <summary>
    ///     <para>Automatically looks up the view that corresponds to the current view-model, using two strategies:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>It first looks to see if there is a mapping registered for that view-model.</description>
    ///         </item>
    ///         <item>
    ///             <description>If not it will fallback to the convention based approach.</description>
    ///         </item>
    ///     </list>
    /// </summary>
    /// <param name="viewModel">The view-model object.</param>
    /// <param name="setDataContextCallback">The call back to use to create the binding between the view and view-model.</param>
    /// <returns>The located view; or <see langword="null"/> if no matching view could be found.</returns>
    public FrameworkElement? LocateViewForViewModel(object viewModel,
                                                    Action<object, FrameworkElement>? setDataContextCallback = default)
    {
        // Try mappings first
        var view = GetViewForViewModelFromFactory(viewModel);

        // Try to use view type
        if (view is null)
        {
            // Check type mappings
            // Fallback to convention based
            var viewType = LocateViewTypeForViewModelType(viewModel.GetType());

            if (viewType is null)
            {
                return BuildPlaceholderView(viewModel.GetType());
            }

            view = _defaultViewFactory(viewType);
        }

        if (view is not null)
        {
            setDataContextCallback?.Invoke(viewModel, view);

            if (CallInitializeComponent)
            {
                InitializeComponent(view);
            }
        }

        return view;
    }

    /// <summary>Locates the type of the view type for view-model.</summary>
    /// <param name="viewModelType">Type of the view-model.</param>
    /// <returns>The located view type; or <see langword="null"/> if no matching type could be found.</returns>
    public Type? LocateViewTypeForViewModelType(Type viewModelType)
    {
        var viewModelKey = viewModelType.ToString();

        return _typeFactories.TryGetValue(viewModelKey, out var value)
                   ? value
                   : _defaultViewModelTypeToViewTypeResolver(viewModelType);
    }

    /// <summary>Registers the view factory for the specified view-model type.</summary>
    /// <typeparam name="TViewModel">The view-model type.</typeparam>
    /// <param name="factory">The view factory.</param>
    public void Register<TViewModel>(Func<FrameworkElement> factory)
    {
        Register(typeof(TViewModel).ToString(), factory);
    }

    /// <summary>Registers the view factory for the specified view-model type name.</summary>
    /// <param name="viewModelTypeName">The name of the view-model type.</param>
    /// <param name="factory">The view factory.</param>
    public void Register(string viewModelTypeName, Func<FrameworkElement> factory)
    {
        _factories[viewModelTypeName] = factory;
    }

    /// <summary>Registers a view type for the specified view-model type.</summary>
    /// <typeparam name="TViewModel">The view-model type.</typeparam>
    /// <typeparam name="TView">The view type.</typeparam>
    public void Register<TViewModel, TView>()
    {
        var viewModelType = typeof(TViewModel);
        var viewType = typeof(TView);

        Register(viewModelType.ToString(), viewType);
    }

    /// <summary>Registers a view type for the specified view-model.</summary>
    /// <param name="viewModelTypeName">The view-model type name.</param>
    /// <param name="viewType">The view type.</param>
    public void Register(string viewModelTypeName, Type viewType)
    {
        _typeFactories[viewModelTypeName] = viewType;
    }

    /// <summary>Sets the default view factory.</summary>
    /// <param name="viewFactory">The view-model factory which provides the view-model type as a parameter.</param>
    public void SetDefaultViewModelFactory(Func<Type, FrameworkElement> viewFactory)
    {
        _defaultViewFactory = viewFactory;
    }

    /// <summary>Sets the default view-model type to view type resolver.</summary>
    /// <param name="viewModelTypeToViewTypeResolver">The view type to view-model type resolver.</param>
    public void SetDefaultViewModelTypeToViewTypeResolver(Func<Type, Type> viewModelTypeToViewTypeResolver)
    {
        _defaultViewModelTypeToViewTypeResolver = viewModelTypeToViewTypeResolver;
    }

    private static FrameworkElement BuildDialogPlaceholderView(Type viewModelType)
    {
        var yesButton = new Button
                        {
                            Width = 200, Height = 50, Margin = new Thickness(uniformLength: 10), Content = "Yes",
                        };

        CloseDialog.SetResult(yesButton, DialogResult.Yes);

        var noButton = new Button
                       {
                           Width = 200, Height = 50, Margin = new Thickness(uniformLength: 10), Content = "No",
                       };

        CloseDialog.SetResult(noButton, DialogResult.No);

        var cancelButton = new Button
                           {
                               Width = 200, Height = 50, Margin = new Thickness(uniformLength: 10), Content = "Cancel",
                           };

        CloseDialog.SetResult(cancelButton, DialogResult.Cancel);

        var buttonGrid = new UniformGrid { Columns = 3, Children = { yesButton, noButton, cancelButton } };
        var textBlockGrid = new Grid { Height = 100, Children = { CreatePlaceholderTextBlock(viewModelType) } };

        var stackPanel = new StackPanel { Children = { textBlockGrid, buttonGrid } };

        return new Border
               {
                   HorizontalAlignment = HorizontalAlignment.Center,
                   VerticalAlignment = VerticalAlignment.Center,
                   Background = Brushes.Maroon,
                   BorderThickness = new Thickness(uniformLength: 2),
                   Child = stackPanel,
               };
    }

    private static FrameworkElement BuildPlaceholderView(Type viewModelType)
    {
        if (viewModelType.IsDerivedFromOrImplements(typeof(DialogScreen)))
        {
            return BuildDialogPlaceholderView(viewModelType);
        }

        return new Border { Background = Brushes.Maroon, Child = CreatePlaceholderTextBlock(viewModelType) };
    }

    private static TextBlock CreatePlaceholderTextBlock(Type viewModelType)
    {
        return new TextBlock
               {
                   Margin = new Thickness(uniformLength: 5),
                   Foreground = Brushes.BlanchedAlmond,
                   FontWeight = FontWeights.DemiBold,
                   FontSize = 16,
                   HorizontalAlignment = HorizontalAlignment.Center,
                   VerticalAlignment = VerticalAlignment.Center,
                   Text = $"Placeholder view for {viewModelType.Name}",
               };
    }

    private static void InitializeComponent(object element)
    {
        // When a view does not contain a code-behind file, we need to automatically call InitializeComponent.
        // The auto-generated InitializeComponent is protected against being called multiple times, so this is always safe.
        var method = element.GetType()
                            .GetMethod(name: "InitializeComponent", BindingFlags.Public | BindingFlags.Instance);

        method?.Invoke(element, parameters: null);
    }

    private FrameworkElement? GetViewForViewModelFromFactory(object viewModel)
    {
        var viewModelKey = viewModel.GetType().ToString();

        return _factories.TryGetValue(viewModelKey, out var value) ? value() : null;
    }
}