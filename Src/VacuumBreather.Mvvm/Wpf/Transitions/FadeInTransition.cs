using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>A fade-in <see cref="ITransition"/> effect.</summary>
/// <seealso cref="TransitionBase"/>
[PublicAPI]
public class FadeInTransition : FadeTransitionBase
{
    /// <summary>Initializes a new instance of the <see cref="FadeInTransition"/> class.</summary>
    public FadeInTransition()
        : base(FadeTransitionType.FadeIn)
    {
    }
}