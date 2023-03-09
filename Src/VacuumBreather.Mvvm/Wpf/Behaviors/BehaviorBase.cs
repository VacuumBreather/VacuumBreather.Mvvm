using System;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace VacuumBreather.Mvvm.Wpf.Behaviors;

/// <summary>
///     Base class for behaviors. Compared to <see cref="Microsoft.Xaml.Behaviors.Behavior{T}"/> it provides custom
///     setup and cleanup methods to avoid possible memory leaks resulting from event subscriptions to objects other than
///     the element the behavior is attached to.
/// </summary>
/// <typeparam name="T">The type of element this behavior can be attached to.</typeparam>
/// <seealso cref="Microsoft.Xaml.Behaviors.Behavior{T}"/>
public abstract class BehaviorBase<T> : Behavior<T>
    where T : FrameworkElement
{
    private bool _isSetup = true;
    private bool _isHookedUp;
    private WeakReference? _weakTarget;

    /// <summary>Override to provide any cleanup logic for this behavior.</summary>
    protected virtual void OnCleanup()
    {
    }

    /// <summary>Override to provide any setup logic for this behavior.</summary>
    protected virtual void OnSetup()
    {
    }

    /// <summary>Called when the current <see cref="Microsoft.Xaml.Behaviors.Behavior{T}"/> object is modified.</summary>
    protected override void OnChanged()
    {
        var target = AssociatedObject;

        if (target != null)
        {
            HookupBehavior(target);
        }
        else
        {
            UnhookBehavior();
        }
    }

    private void CleanupBehavior()
    {
        if (!_isSetup)
        {
            return;
        }

        _isSetup = false;
        OnCleanup();
    }

    private void HookupBehavior(T target)
    {
        if (_isHookedUp)
        {
            return;
        }

        _weakTarget = new WeakReference(target);
        _isHookedUp = true;

        target.Unloaded += OnTargetUnloaded;
        target.Loaded += OnTargetLoaded;

        SetupBehavior();
    }

    private void OnTargetLoaded(object sender, RoutedEventArgs e)
    {
        SetupBehavior();
    }

    private void OnTargetUnloaded(object sender, RoutedEventArgs e)
    {
        CleanupBehavior();
    }

    private void SetupBehavior()
    {
        if (_isSetup)
        {
            return;
        }

        _isSetup = true;
        OnSetup();
    }

    private void UnhookBehavior()
    {
        if (!_isHookedUp)
        {
            return;
        }

        _isHookedUp = false;

        var target = AssociatedObject ?? (T?)_weakTarget?.Target;

        if (target != null)
        {
            target.Unloaded -= OnTargetUnloaded;
            target.Loaded -= OnTargetLoaded;
        }

        CleanupBehavior();
    }
}