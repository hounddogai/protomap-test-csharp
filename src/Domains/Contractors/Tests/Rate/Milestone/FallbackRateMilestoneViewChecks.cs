namespace Acme.Workforce.Contractors.Tests.Rate.Milestone;

using Acme.Workforce.Contractors.Rate.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class FallbackRateMilestoneViewChecks
{
    public static void FallbackRateMilestoneViewShowsTheStatusInCapitals()
    {
        var item = FallbackRateMilestone.NewFallbackRateMilestone("code-443", "name-30");
        var view = FallbackRateMilestoneView.ToFallbackRateMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackRateMilestoneView.ToFallbackRateMilestoneViews(new List<FallbackRateMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackRateMilestoneViewShowsTheStatusInCapitals();
    }
}
