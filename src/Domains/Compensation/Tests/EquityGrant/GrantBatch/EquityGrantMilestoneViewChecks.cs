namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantBatch;

using Acme.Workforce.Compensation.EquityGrant.GrantBatch;

/// <summary>Checks of grant batch code, run by the project's runner.</summary>
public static class EquityGrantMilestoneViewChecks
{
    public static void EquityGrantMilestoneViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantMilestone.NewEquityGrantMilestone("code-553", "title-740");
        var view = EquityGrantMilestoneView.ToEquityGrantMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantMilestoneView.ToEquityGrantMilestoneViews(new List<EquityGrantMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantMilestoneViewShowsTheStatusInCapitals();
    }
}
