namespace Acme.Workforce.Orgdesign.Tests.CostModel.Assignment;

using Acme.Workforce.Orgdesign.CostModel.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CostModelBatchChecks
{
    public static void CostModelBatchStartsInNew()
    {
        var item = CostModelBatch.NewCostModelBatch("code-697", "name-888");
        Check.Equal(item.Status, CostModelBatchStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelBatchMovesToRunning()
    {
        var item = CostModelBatch.NewCostModelBatch("code-697", "name-888");
        try
        {
            item.MoveTo(CostModelBatchStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelBatchStatus.Running, "status");
    }

    public static void CostModelBatchRejectsMovesFromFailed()
    {
        var item = CostModelBatch.NewCostModelBatch("code-697", "name-888");
        item.Status = CostModelBatchStatus.Failed;
        Check.Fails(() => item.MoveTo(CostModelBatchStatus.New), "move");
    }

    public static void RunAll()
    {
        CostModelBatchStartsInNew();
        CostModelBatchMovesToRunning();
        CostModelBatchRejectsMovesFromFailed();
    }
}
