namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleReview;

using Acme.Workforce.Compensation.MeritCycle.CycleReview;

/// <summary>Checks of cycle review code, run by the project's runner.</summary>
public static class LegacyMeritCycleReviewViewChecks
{
    public static void LegacyMeritCycleReviewViewShowsTheStatusInCapitals()
    {
        var item = LegacyMeritCycleReview.NewLegacyMeritCycleReview("code-572", "name-40");
        var view = LegacyMeritCycleReviewView.ToLegacyMeritCycleReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyMeritCycleReviewView.ToLegacyMeritCycleReviewViews(new List<LegacyMeritCycleReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyMeritCycleReviewViewShowsTheStatusInCapitals();
    }
}
