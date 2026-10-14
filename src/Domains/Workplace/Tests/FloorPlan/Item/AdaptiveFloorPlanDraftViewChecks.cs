namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class AdaptiveFloorPlanDraftViewChecks
{
    public static void AdaptiveFloorPlanDraftViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveFloorPlanDraft.NewAdaptiveFloorPlanDraft("code-644", "name-301");
        var view = AdaptiveFloorPlanDraftView.ToAdaptiveFloorPlanDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveFloorPlanDraftView.ToAdaptiveFloorPlanDraftViews(new List<AdaptiveFloorPlanDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveFloorPlanDraftViewShowsTheStatusInCapitals();
    }
}
