namespace Acme.Workforce.Compensation.Tests.BonusPool.Allocation;

using Acme.Workforce.Compensation.BonusPool.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BonusPoolAllocationChecks
{
    public static void BonusPoolAllocationStartsInDraft()
    {
        var item = BonusPoolAllocation.NewBonusPoolAllocation("code-496", "name-102");
        Check.Equal(item.Status, BonusPoolAllocationStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolAllocationMovesToPending()
    {
        var item = BonusPoolAllocation.NewBonusPoolAllocation("code-496", "name-102");
        try
        {
            item.MoveTo(BonusPoolAllocationStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolAllocationStatus.Pending, "status");
    }

    public static void BonusPoolAllocationRejectsMovesFromClosed()
    {
        var item = BonusPoolAllocation.NewBonusPoolAllocation("code-496", "name-102");
        item.Status = BonusPoolAllocationStatus.Closed;
        Check.Fails(() => item.MoveTo(BonusPoolAllocationStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BonusPoolAllocationStartsInDraft();
        BonusPoolAllocationMovesToPending();
        BonusPoolAllocationRejectsMovesFromClosed();
    }
}
