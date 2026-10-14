namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class FallbackActionPlanDraftViewChecks
{
    public static void FallbackActionPlanDraftViewShowsTheStatusInCapitals()
    {
        var item = FallbackActionPlanDraft.NewFallbackActionPlanDraft("code-538", "label-842");
        var view = FallbackActionPlanDraftView.ToFallbackActionPlanDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackActionPlanDraftView.ToFallbackActionPlanDraftViews(new List<FallbackActionPlanDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackActionPlanDraftViewShowsTheStatusInCapitals();
    }
}
