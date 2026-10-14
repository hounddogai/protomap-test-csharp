namespace Acme.Workforce.Orgdesign.Tests.Reorg.Bundle;

using Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ReorgCycleViewChecks
{
    public static void ReorgCycleViewShowsTheStatusInCapitals()
    {
        var item = ReorgCycle.NewReorgCycle("code-338", "label-539");
        var view = ReorgCycleView.ToReorgCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgCycleView.ToReorgCycleViews(new List<ReorgCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgCycleViewShowsTheStatusInCapitals();
    }
}
