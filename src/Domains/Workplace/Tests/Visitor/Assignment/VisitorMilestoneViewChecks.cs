namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorMilestoneViewChecks
{
    public static void VisitorMilestoneViewShowsTheStatusInCapitals()
    {
        var item = VisitorMilestone.NewVisitorMilestone("code-514", "title-147");
        var view = VisitorMilestoneView.ToVisitorMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorMilestoneView.ToVisitorMilestoneViews(new List<VisitorMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorMilestoneViewShowsTheStatusInCapitals();
    }
}
