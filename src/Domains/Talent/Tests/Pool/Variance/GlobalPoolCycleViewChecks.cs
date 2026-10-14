namespace Acme.Workforce.Talent.Tests.Pool.Variance;

using Acme.Workforce.Talent.Pool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class GlobalPoolCycleViewChecks
{
    public static void GlobalPoolCycleViewShowsTheStatusInCapitals()
    {
        var item = GlobalPoolCycle.NewGlobalPoolCycle("code-549", "name-411");
        var view = GlobalPoolCycleView.ToGlobalPoolCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalPoolCycleView.ToGlobalPoolCycleViews(new List<GlobalPoolCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalPoolCycleViewShowsTheStatusInCapitals();
    }
}
