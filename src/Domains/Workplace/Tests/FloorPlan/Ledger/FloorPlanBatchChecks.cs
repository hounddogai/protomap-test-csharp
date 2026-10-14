namespace Acme.Workforce.Workplace.Tests.FloorPlan.Ledger;

using Acme.Workforce.Workplace.FloorPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FloorPlanBatchChecks
{
    public static void FloorPlanBatchStartsInCreated()
    {
        var item = FloorPlanBatch.NewFloorPlanBatch("code-504", "name-614");
        Check.Equal(item.Status, FloorPlanBatchStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanBatchMovesToPublished()
    {
        var item = FloorPlanBatch.NewFloorPlanBatch("code-504", "name-614");
        try
        {
            item.MoveTo(FloorPlanBatchStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanBatchStatus.Published, "status");
    }

    public static void FloorPlanBatchRejectsMovesFromRetired()
    {
        var item = FloorPlanBatch.NewFloorPlanBatch("code-504", "name-614");
        item.Status = FloorPlanBatchStatus.Retired;
        Check.Fails(() => item.MoveTo(FloorPlanBatchStatus.Created), "move");
    }

    public static void RunAll()
    {
        FloorPlanBatchStartsInCreated();
        FloorPlanBatchMovesToPublished();
        FloorPlanBatchRejectsMovesFromRetired();
    }
}
