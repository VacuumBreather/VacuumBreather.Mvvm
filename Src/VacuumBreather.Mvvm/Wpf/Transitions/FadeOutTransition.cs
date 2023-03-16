using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>A fade-out <see cref="ITransition"/> effect.</summary>
/// <seealso cref="TransitionBase"/>
[PublicAPI]
public class FadeOutTransition : FadeTransitionBase
{
    /// <summary>Initializes a new instance of the <see cref="FadeOutTransition"/> class.</summary>
    public FadeOutTransition()
        : base(FadeTransitionType.FadeOut)
    {
    }
}