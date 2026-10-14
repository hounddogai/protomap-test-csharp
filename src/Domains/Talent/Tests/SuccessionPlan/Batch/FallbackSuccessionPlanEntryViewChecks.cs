namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class FallbackSuccessionPlanEntryViewChecks
{
    public static void FallbackSuccessionPlanEntryViewShowsTheStatusInCapitals()
    {
        var item = FallbackSuccessionPlanEntry.NewFallbackSuccessionPlanEntry("code-841", "label-594");
        var view = FallbackSuccessionPlanEntryView.ToFallbackSuccessionPlanEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackSuccessionPlanEntryView.ToFallbackSuccessionPlanEntryViews(new List<FallbackSuccessionPlanEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackSuccessionPlanEntryViewShowsTheStatusInCapitals();
    }
}
