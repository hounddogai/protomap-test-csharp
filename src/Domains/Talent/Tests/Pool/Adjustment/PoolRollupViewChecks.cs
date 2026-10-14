namespace Acme.Workforce.Talent.Tests.Pool.Adjustment;

using Acme.Workforce.Talent.Pool.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PoolRollupViewChecks
{
    public static void PoolRollupViewShowsTheStatusInCapitals()
    {
        var item = PoolRollup.NewPoolRollup("code-887", "title-915");
        var view = PoolRollupView.ToPoolRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PoolRollupView.ToPoolRollupViews(new List<PoolRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PoolRollupViewShowsTheStatusInCapitals();
    }
}
