namespace Acme.Workforce.Orgdesign.Tests.CostModel.Checkpoint;

using Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class CostModelAllocationChecks
{
    public static void CostModelAllocationStartsInIdle()
    {
        var item = CostModelAllocation.NewCostModelAllocation("code-548", "label-346");
        Check.Equal(item.Status, CostModelAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelAllocationMovesToDraining()
    {
        var item = CostModelAllocation.NewCostModelAllocation("code-548", "label-346");
        try
        {
            item.MoveTo(CostModelAllocationStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelAllocationStatus.Draining, "status");
    }

    public static void CostModelAllocationRejectsMovesFromStopped()
    {
        var item = CostModelAllocation.NewCostModelAllocation("code-548", "label-346");
        item.Status = CostModelAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(CostModelAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CostModelAllocationStartsInIdle();
        CostModelAllocationMovesToDraining();
        CostModelAllocationRejectsMovesFromStopped();
    }
}
