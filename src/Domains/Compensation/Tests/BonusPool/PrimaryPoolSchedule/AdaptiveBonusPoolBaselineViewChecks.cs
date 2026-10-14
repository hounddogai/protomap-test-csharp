namespace Acme.Workforce.Compensation.Tests.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>Checks of primary pool schedule code, run by the project's runner.</summary>
public static class AdaptiveBonusPoolBaselineViewChecks
{
    public static void AdaptiveBonusPoolBaselineViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveBonusPoolBaseline.NewAdaptiveBonusPoolBaseline("code-808", "label-635");
        var view = AdaptiveBonusPoolBaselineView.ToAdaptiveBonusPoolBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveBonusPoolBaselineView.ToAdaptiveBonusPoolBaselineViews(new List<AdaptiveBonusPoolBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveBonusPoolBaselineViewShowsTheStatusInCapitals();
    }
}
