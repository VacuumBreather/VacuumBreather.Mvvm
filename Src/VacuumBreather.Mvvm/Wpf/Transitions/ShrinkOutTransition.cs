using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>A <see cref="ITransition"/> effect which shrinks the <see cref="ITransitionSubject"/> and fades it out.</summary>
/// <seealso cref="ZoomTransitionBase"/>
/// <seealso cref="ITransition"/>
[PublicAPI]
public class ShrinkOutTransition : ZoomTransitionBase
{
    /// <summary>Initializes a new instance of the <see cref="ShrinkOutTransition"/> class.</summary>
    public ShrinkOutTransition()
        : base(startScale: 1.0, endScale: 0.8, startOpacity: 1.0, endOpacity: 0.0)
    {
    }
}