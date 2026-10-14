namespace Acme.Workforce.Engagement.Tests.ActionPlan.Notice;

using Acme.Workforce.Engagement.ActionPlan.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ActionPlanNoticeViewChecks
{
    public static void ActionPlanNoticeViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanNotice.NewActionPlanNotice("code-653", "name-323");
        var view = ActionPlanNoticeView.ToActionPlanNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanNoticeView.ToActionPlanNoticeViews(new List<ActionPlanNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanNoticeViewShowsTheStatusInCapitals();
    }
}
