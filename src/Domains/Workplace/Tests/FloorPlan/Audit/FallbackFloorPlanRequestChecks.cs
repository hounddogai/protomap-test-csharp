namespace Acme.Workforce.Workplace.Tests.FloorPlan.Audit;

using Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FallbackFloorPlanRequestChecks
{
    public static void FallbackFloorPlanRequestStartsInOpen()
    {
        var item = FallbackFloorPlanRequest.NewFallbackFloorPlanRequest("code-219", "label-774");
        Check.Equal(item.Status, FallbackFloorPlanRequestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackFloorPlanRequestMovesToInReview()
    {
        var item = FallbackFloorPlanRequest.NewFallbackFloorPlanRequest("code-219", "label-774");
        try
        {
            item.MoveTo(FallbackFloorPlanRequestStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackFloorPlanRequestStatus.InReview, "status");
    }

    public static void FallbackFloorPlanRequestRejectsMovesFromArchived()
    {
        var item = FallbackFloorPlanRequest.NewFallbackFloorPlanRequest("code-219", "label-774");
        item.Status = FallbackFloorPlanRequestStatus.Archived;
        Check.Fails(() => item.MoveTo(FallbackFloorPlanRequestStatus.Open), "move");
    }

    public static void RunAll()
    {
        FallbackFloorPlanRequestStartsInOpen();
        FallbackFloorPlanRequestMovesToInReview();
        FallbackFloorPlanRequestRejectsMovesFromArchived();
    }
}
