namespace Acme.Workforce.Talent.Tests.Pool.Threshold;

using Acme.Workforce.Talent.Pool.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PoolThresholdViewChecks
{
    public static void PoolThresholdViewShowsTheStatusInCapitals()
    {
        var item = PoolThreshold.NewPoolThreshold("code-954", "name-969");
        var view = PoolThresholdView.ToPoolThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PoolThresholdView.ToPoolThresholdViews(new List<PoolThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PoolThresholdViewShowsTheStatusInCapitals();
    }
}
