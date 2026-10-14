namespace Acme.Workforce.Talent.Tests.Pool.PoolWindow;

using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Checks of pool window code, run by the project's runner.</summary>
public static class PoolCycleViewChecks
{
    public static void PoolCycleViewShowsTheStatusInCapitals()
    {
        var item = PoolCycle.NewPoolCycle("code-481", "title-642");
        var view = PoolCycleView.ToPoolCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PoolCycleView.ToPoolCycleViews(new List<PoolCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PoolCycleViewShowsTheStatusInCapitals();
    }
}
