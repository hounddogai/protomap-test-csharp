namespace Acme.Workforce.Workplace.Tests.FloorPlan.Ledger;

using Acme.Workforce.Workplace.FloorPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FloorPlanDraftChecks
{
    public static void FloorPlanDraftStartsInPlanned()
    {
        var item = FloorPlanDraft.NewFloorPlanDraft("code-862", "name-343");
        Check.Equal(item.Status, FloorPlanDraftStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanDraftMovesToInProgress()
    {
        var item = FloorPlanDraft.NewFloorPlanDraft("code-862", "name-343");
        try
        {
            item.MoveTo(FloorPlanDraftStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanDraftStatus.InProgress, "status");
    }

    public static void FloorPlanDraftRejectsMovesFromCancelled()
    {
        var item = FloorPlanDraft.NewFloorPlanDraft("code-862", "name-343");
        item.Status = FloorPlanDraftStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FloorPlanDraftStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FloorPlanDraftStartsInPlanned();
        FloorPlanDraftMovesToInProgress();
        FloorPlanDraftRejectsMovesFromCancelled();
    }
}
