namespace Acme.Workforce.Engagement.Tests.ActionPlan.Metric;

using Acme.Workforce.Engagement.ActionPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedActionPlanDraftViewChecks
{
    public static void CappedActionPlanDraftViewShowsTheStatusInCapitals()
    {
        var item = CappedActionPlanDraft.NewCappedActionPlanDraft("code-950", "name-918");
        var view = CappedActionPlanDraftView.ToCappedActionPlanDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedActionPlanDraftView.ToCappedActionPlanDraftViews(new List<CappedActionPlanDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedActionPlanDraftViewShowsTheStatusInCapitals();
    }
}
