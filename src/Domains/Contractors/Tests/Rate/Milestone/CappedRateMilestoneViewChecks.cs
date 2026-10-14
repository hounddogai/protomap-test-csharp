namespace Acme.Workforce.Contractors.Tests.Rate.Milestone;

using Acme.Workforce.Contractors.Rate.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class CappedRateMilestoneViewChecks
{
    public static void CappedRateMilestoneViewShowsTheStatusInCapitals()
    {
        var item = CappedRateMilestone.NewCappedRateMilestone("code-428", "label-654");
        var view = CappedRateMilestoneView.ToCappedRateMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedRateMilestoneView.ToCappedRateMilestoneViews(new List<CappedRateMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedRateMilestoneViewShowsTheStatusInCapitals();
    }
}
