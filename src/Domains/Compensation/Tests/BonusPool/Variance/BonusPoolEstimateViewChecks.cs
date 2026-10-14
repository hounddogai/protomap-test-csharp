namespace Acme.Workforce.Compensation.Tests.BonusPool.Variance;

using Acme.Workforce.Compensation.BonusPool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BonusPoolEstimateViewChecks
{
    public static void BonusPoolEstimateViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolEstimate.NewBonusPoolEstimate("code-378", "name-312");
        var view = BonusPoolEstimateView.ToBonusPoolEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolEstimateView.ToBonusPoolEstimateViews(new List<BonusPoolEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolEstimateViewShowsTheStatusInCapitals();
    }
}
