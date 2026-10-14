namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleSegment;

using Acme.Workforce.Compensation.MeritCycle.CycleSegment;

/// <summary>Checks of cycle segment code, run by the project's runner.</summary>
public static class AdaptiveMeritCycleMappingViewChecks
{
    public static void AdaptiveMeritCycleMappingViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveMeritCycleMapping.NewAdaptiveMeritCycleMapping("code-243", "name-690");
        var view = AdaptiveMeritCycleMappingView.ToAdaptiveMeritCycleMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveMeritCycleMappingView.ToAdaptiveMeritCycleMappingViews(new List<AdaptiveMeritCycleMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveMeritCycleMappingViewShowsTheStatusInCapitals();
    }
}
