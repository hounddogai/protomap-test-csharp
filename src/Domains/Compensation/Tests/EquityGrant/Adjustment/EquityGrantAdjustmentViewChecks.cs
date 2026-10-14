namespace Acme.Workforce.Compensation.Tests.EquityGrant.Adjustment;

using Acme.Workforce.Compensation.EquityGrant.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EquityGrantAdjustmentViewChecks
{
    public static void EquityGrantAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantAdjustment.NewEquityGrantAdjustment("code-494", "title-345");
        var view = EquityGrantAdjustmentView.ToEquityGrantAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantAdjustmentView.ToEquityGrantAdjustmentViews(new List<EquityGrantAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantAdjustmentViewShowsTheStatusInCapitals();
    }
}
