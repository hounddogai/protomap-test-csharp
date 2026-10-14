namespace Acme.Workforce.Talent.Tests.Pool.Window;

using Acme.Workforce.Talent.Pool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PoolVarianceViewChecks
{
    public static void PoolVarianceViewShowsTheStatusInCapitals()
    {
        var item = PoolVariance.NewPoolVariance("code-534", "label-899");
        var view = PoolVarianceView.ToPoolVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PoolVarianceView.ToPoolVarianceViews(new List<PoolVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PoolVarianceViewShowsTheStatusInCapitals();
    }
}
