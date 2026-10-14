namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAllocation;

using Acme.Workforce.Payrollops.PayRun.RunAllocation;

/// <summary>Checks of run allocation code, run by the project's runner.</summary>
public static class FallbackPayRunCycleViewChecks
{
    public static void FallbackPayRunCycleViewShowsTheStatusInCapitals()
    {
        var item = FallbackPayRunCycle.NewFallbackPayRunCycle("code-163", "title-577");
        var view = FallbackPayRunCycleView.ToFallbackPayRunCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackPayRunCycleView.ToFallbackPayRunCycleViews(new List<FallbackPayRunCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackPayRunCycleViewShowsTheStatusInCapitals();
    }
}
