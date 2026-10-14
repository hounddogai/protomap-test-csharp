namespace Acme.Workforce.Engagement.Tests.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

/// <summary>Checks of weighted plan segment code, run by the project's runner.</summary>
public static class GlobalActionPlanEntryViewChecks
{
    public static void GlobalActionPlanEntryViewShowsTheStatusInCapitals()
    {
        var item = GlobalActionPlanEntry.NewGlobalActionPlanEntry("code-111", "title-153");
        var view = GlobalActionPlanEntryView.ToGlobalActionPlanEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalActionPlanEntryView.ToGlobalActionPlanEntryViews(new List<GlobalActionPlanEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalActionPlanEntryViewShowsTheStatusInCapitals();
    }
}
