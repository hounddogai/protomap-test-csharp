namespace Acme.Workforce.Compensation.Tests.BonusPool.Entry;

using Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BonusPoolSegmentViewChecks
{
    public static void BonusPoolSegmentViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolSegment.NewBonusPoolSegment("code-848", "label-929");
        var view = BonusPoolSegmentView.ToBonusPoolSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolSegmentView.ToBonusPoolSegmentViews(new List<BonusPoolSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolSegmentViewShowsTheStatusInCapitals();
    }
}
