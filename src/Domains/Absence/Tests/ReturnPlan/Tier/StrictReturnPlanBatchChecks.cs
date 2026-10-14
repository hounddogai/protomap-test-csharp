namespace Acme.Workforce.Absence.Tests.ReturnPlan.Tier;

using Acme.Workforce.Absence.ReturnPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StrictReturnPlanBatchChecks
{
    public static void StrictReturnPlanBatchStartsInNew()
    {
        var item = StrictReturnPlanBatch.NewStrictReturnPlanBatch("code-608", "title-977");
        Check.Equal(item.Status, StrictReturnPlanBatchStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictReturnPlanBatchMovesToRunning()
    {
        var item = StrictReturnPlanBatch.NewStrictReturnPlanBatch("code-608", "title-977");
        try
        {
            item.MoveTo(StrictReturnPlanBatchStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictReturnPlanBatchStatus.Running, "status");
    }

    public static void StrictReturnPlanBatchRejectsMovesFromFailed()
    {
        var item = StrictReturnPlanBatch.NewStrictReturnPlanBatch("code-608", "title-977");
        item.Status = StrictReturnPlanBatchStatus.Failed;
        Check.Fails(() => item.MoveTo(StrictReturnPlanBatchStatus.New), "move");
    }

    public static void RunAll()
    {
        StrictReturnPlanBatchStartsInNew();
        StrictReturnPlanBatchMovesToRunning();
        StrictReturnPlanBatchRejectsMovesFromFailed();
    }
}
