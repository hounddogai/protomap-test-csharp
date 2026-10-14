namespace Acme.Workforce.Workplace.Tests.Visitor.Cycle;

using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveVisitorCycleViewChecks
{
    public static void AdaptiveVisitorCycleViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveVisitorCycle.NewAdaptiveVisitorCycle("code-891", "title-896");
        var view = AdaptiveVisitorCycleView.ToAdaptiveVisitorCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveVisitorCycleView.ToAdaptiveVisitorCycleViews(new List<AdaptiveVisitorCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveVisitorCycleViewShowsTheStatusInCapitals();
    }
}
