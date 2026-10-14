namespace Acme.Workforce.Engagement.Tests.ActionPlan.Group;

using Acme.Workforce.Engagement.ActionPlan.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ManualActionPlanEntryViewChecks
{
    public static void ManualActionPlanEntryViewShowsTheStatusInCapitals()
    {
        var item = ManualActionPlanEntry.NewManualActionPlanEntry("code-354", "label-100");
        var view = ManualActionPlanEntryView.ToManualActionPlanEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualActionPlanEntryView.ToManualActionPlanEntryViews(new List<ManualActionPlanEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualActionPlanEntryViewShowsTheStatusInCapitals();
    }
}
