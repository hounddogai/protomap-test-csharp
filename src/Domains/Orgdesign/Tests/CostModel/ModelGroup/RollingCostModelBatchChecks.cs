namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelGroup;

using Acme.Workforce.Orgdesign.CostModel.ModelGroup;

/// <summary>Checks of model group code, run by the project's runner.</summary>
public static class RollingCostModelBatchChecks
{
    public static void RollingCostModelBatchStartsInIdle()
    {
        var item = RollingCostModelBatch.NewRollingCostModelBatch("code-378", "label-206");
        Check.Equal(item.Status, RollingCostModelBatchStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingCostModelBatchMovesToStopped()
    {
        var item = RollingCostModelBatch.NewRollingCostModelBatch("code-378", "label-206");
        try
        {
            item.MoveTo(RollingCostModelBatchStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingCostModelBatchStatus.Stopped, "status");
    }

    public static void RollingCostModelBatchRejectsMovesFromStopped()
    {
        var item = RollingCostModelBatch.NewRollingCostModelBatch("code-378", "label-206");
        item.Status = RollingCostModelBatchStatus.Stopped;
        Check.Fails(() => item.MoveTo(RollingCostModelBatchStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RollingCostModelBatchStartsInIdle();
        RollingCostModelBatchMovesToStopped();
        RollingCostModelBatchRejectsMovesFromStopped();
    }
}
