namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class AdaptiveFloorPlanDraftChecks
{
    public static void AdaptiveFloorPlanDraftStartsInNew()
    {
        var item = AdaptiveFloorPlanDraft.NewAdaptiveFloorPlanDraft("code-644", "name-301");
        Check.Equal(item.Status, AdaptiveFloorPlanDraftStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveFloorPlanDraftMovesToQueued()
    {
        var item = AdaptiveFloorPlanDraft.NewAdaptiveFloorPlanDraft("code-644", "name-301");
        try
        {
            item.MoveTo(AdaptiveFloorPlanDraftStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveFloorPlanDraftStatus.Queued, "status");
    }

    public static void AdaptiveFloorPlanDraftRejectsMovesFromFailed()
    {
        var item = AdaptiveFloorPlanDraft.NewAdaptiveFloorPlanDraft("code-644", "name-301");
        item.Status = AdaptiveFloorPlanDraftStatus.Failed;
        Check.Fails(() => item.MoveTo(AdaptiveFloorPlanDraftStatus.New), "move");
    }

    public static void RunAll()
    {
        AdaptiveFloorPlanDraftStartsInNew();
        AdaptiveFloorPlanDraftMovesToQueued();
        AdaptiveFloorPlanDraftRejectsMovesFromFailed();
    }
}
