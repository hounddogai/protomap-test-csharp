namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class FallbackCareerPathAdjustmentViewChecks
{
    public static void FallbackCareerPathAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = FallbackCareerPathAdjustment.NewFallbackCareerPathAdjustment("code-826", "label-343");
        var view = FallbackCareerPathAdjustmentView.ToFallbackCareerPathAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackCareerPathAdjustmentView.ToFallbackCareerPathAdjustmentViews(new List<FallbackCareerPathAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackCareerPathAdjustmentViewShowsTheStatusInCapitals();
    }
}
