namespace Acme.Workforce.Compensation.Tests.EquityGrant.Tier;

using Acme.Workforce.Compensation.EquityGrant.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EquityGrantTargetViewChecks
{
    public static void EquityGrantTargetViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantTarget.NewEquityGrantTarget("code-930", "label-40");
        var view = EquityGrantTargetView.ToEquityGrantTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantTargetView.ToEquityGrantTargetViews(new List<EquityGrantTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantTargetViewShowsTheStatusInCapitals();
    }
}
