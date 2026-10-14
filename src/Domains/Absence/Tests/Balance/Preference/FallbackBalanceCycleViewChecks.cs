namespace Acme.Workforce.Absence.Tests.Balance.Preference;

using Acme.Workforce.Absence.Balance.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class FallbackBalanceCycleViewChecks
{
    public static void FallbackBalanceCycleViewShowsTheStatusInCapitals()
    {
        var item = FallbackBalanceCycle.NewFallbackBalanceCycle("code-430", "label-475");
        var view = FallbackBalanceCycleView.ToFallbackBalanceCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackBalanceCycleView.ToFallbackBalanceCycleViews(new List<FallbackBalanceCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackBalanceCycleViewShowsTheStatusInCapitals();
    }
}
