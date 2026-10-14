namespace Acme.Workforce.Workplace.Tests.FloorPlan.Ledger;

using Acme.Workforce.Workplace.FloorPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FloorPlanDraftViewChecks
{
    public static void FloorPlanDraftViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanDraft.NewFloorPlanDraft("code-862", "name-343");
        var view = FloorPlanDraftView.ToFloorPlanDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanDraftView.ToFloorPlanDraftViews(new List<FloorPlanDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanDraftViewShowsTheStatusInCapitals();
    }
}
