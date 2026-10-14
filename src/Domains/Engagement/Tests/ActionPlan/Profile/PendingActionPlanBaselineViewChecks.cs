namespace Acme.Workforce.Engagement.Tests.ActionPlan.Profile;

using Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PendingActionPlanBaselineViewChecks
{
    public static void PendingActionPlanBaselineViewShowsTheStatusInCapitals()
    {
        var item = PendingActionPlanBaseline.NewPendingActionPlanBaseline("code-385", "name-465");
        var view = PendingActionPlanBaselineView.ToPendingActionPlanBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingActionPlanBaselineView.ToPendingActionPlanBaselineViews(new List<PendingActionPlanBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingActionPlanBaselineViewShowsTheStatusInCapitals();
    }
}
