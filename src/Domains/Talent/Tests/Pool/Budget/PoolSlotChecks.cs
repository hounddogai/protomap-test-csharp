namespace Acme.Workforce.Talent.Tests.Pool.Budget;

using Acme.Workforce.Talent.Pool.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PoolSlotChecks
{
    public static void PoolSlotStartsInPlanned()
    {
        var item = PoolSlot.NewPoolSlot("code-664", "label-868");
        Check.Equal(item.Status, PoolSlotStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolSlotMovesToScheduled()
    {
        var item = PoolSlot.NewPoolSlot("code-664", "label-868");
        try
        {
            item.MoveTo(PoolSlotStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolSlotStatus.Scheduled, "status");
    }

    public static void PoolSlotRejectsMovesFromCancelled()
    {
        var item = PoolSlot.NewPoolSlot("code-664", "label-868");
        item.Status = PoolSlotStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PoolSlotStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PoolSlotStartsInPlanned();
        PoolSlotMovesToScheduled();
        PoolSlotRejectsMovesFromCancelled();
    }
}
