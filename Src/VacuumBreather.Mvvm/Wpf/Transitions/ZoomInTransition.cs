using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>A <see cref="ITransition"/> effect which zooms the <see cref="ITransitionSubject"/> and fades it in.</summary>
/// <seealso cref="ZoomTransitionBase"/>
/// <seealso cref="ITransition"/>
[PublicAPI]
public class ZoomInTransition : ZoomTransitionBase
{
    /// <summary>Initializes a new instance of the <see cref="ZoomInTransition"/> class.</summary>
    public ZoomInTransition()
        : base(startScale: 0.5, endScale: 1.0, startOpacity: 0.0, endOpacity: 1.0)
    {
    }
}