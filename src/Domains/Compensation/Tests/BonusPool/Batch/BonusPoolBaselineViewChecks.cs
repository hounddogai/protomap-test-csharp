namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolBaselineViewChecks
{
    public static void BonusPoolBaselineViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolBaseline.NewBonusPoolBaseline("code-57", "title-23");
        var view = BonusPoolBaselineView.ToBonusPoolBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolBaselineView.ToBonusPoolBaselineViews(new List<BonusPoolBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolBaselineViewShowsTheStatusInCapitals();
    }
}
