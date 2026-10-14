namespace Acme.Workforce.Compensation.Tests.BonusPool.Review;

using Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class DefaultBonusPoolAllocationChecks
{
    public static void DefaultBonusPoolAllocationStartsInIdle()
    {
        var item = DefaultBonusPoolAllocation.NewDefaultBonusPoolAllocation("code-62", "name-131");
        Check.Equal(item.Status, DefaultBonusPoolAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultBonusPoolAllocationMovesToWarming()
    {
        var item = DefaultBonusPoolAllocation.NewDefaultBonusPoolAllocation("code-62", "name-131");
        try
        {
            item.MoveTo(DefaultBonusPoolAllocationStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultBonusPoolAllocationStatus.Warming, "status");
    }

    public static void DefaultBonusPoolAllocationRejectsMovesFromStopped()
    {
        var item = DefaultBonusPoolAllocation.NewDefaultBonusPoolAllocation("code-62", "name-131");
        item.Status = DefaultBonusPoolAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(DefaultBonusPoolAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DefaultBonusPoolAllocationStartsInIdle();
        DefaultBonusPoolAllocationMovesToWarming();
        DefaultBonusPoolAllocationRejectsMovesFromStopped();
    }
}
