namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelManifest;

using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Checks of model manifest code, run by the project's runner.</summary>
public static class ScheduledCostModelBatchChecks
{
    public static void ScheduledCostModelBatchStartsInNew()
    {
        var item = ScheduledCostModelBatch.NewScheduledCostModelBatch("code-855", "name-74");
        Check.Equal(item.Status, ScheduledCostModelBatchStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledCostModelBatchMovesToRunning()
    {
        var item = ScheduledCostModelBatch.NewScheduledCostModelBatch("code-855", "name-74");
        try
        {
            item.MoveTo(ScheduledCostModelBatchStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledCostModelBatchStatus.Running, "status");
    }

    public static void ScheduledCostModelBatchRejectsMovesFromFailed()
    {
        var item = ScheduledCostModelBatch.NewScheduledCostModelBatch("code-855", "name-74");
        item.Status = ScheduledCostModelBatchStatus.Failed;
        Check.Fails(() => item.MoveTo(ScheduledCostModelBatchStatus.New), "move");
    }

    public static void RunAll()
    {
        ScheduledCostModelBatchStartsInNew();
        ScheduledCostModelBatchMovesToRunning();
        ScheduledCostModelBatchRejectsMovesFromFailed();
    }
}
