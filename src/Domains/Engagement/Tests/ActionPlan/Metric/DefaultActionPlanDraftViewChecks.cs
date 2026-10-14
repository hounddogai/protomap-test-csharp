namespace Acme.Workforce.Engagement.Tests.ActionPlan.Metric;

using Acme.Workforce.Engagement.ActionPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class DefaultActionPlanDraftViewChecks
{
    public static void DefaultActionPlanDraftViewShowsTheStatusInCapitals()
    {
        var item = DefaultActionPlanDraft.NewDefaultActionPlanDraft("code-767", "label-759");
        var view = DefaultActionPlanDraftView.ToDefaultActionPlanDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultActionPlanDraftView.ToDefaultActionPlanDraftViews(new List<DefaultActionPlanDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultActionPlanDraftViewShowsTheStatusInCapitals();
    }
}
