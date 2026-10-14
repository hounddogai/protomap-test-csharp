namespace Acme.Workforce.Contractors.Tests.Rate.Slot;

using Acme.Workforce.Contractors.Rate.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class RollingRateSlotChecks
{
    public static void RollingRateSlotStartsInPlanned()
    {
        var item = RollingRateSlot.NewRollingRateSlot("code-357", "label-916");
        Check.Equal(item.Status, RollingRateSlotStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingRateSlotMovesToScheduled()
    {
        var item = RollingRateSlot.NewRollingRateSlot("code-357", "label-916");
        try
        {
            item.MoveTo(RollingRateSlotStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingRateSlotStatus.Scheduled, "status");
    }

    public static void RollingRateSlotRejectsMovesFromCancelled()
    {
        var item = RollingRateSlot.NewRollingRateSlot("code-357", "label-916");
        item.Status = RollingRateSlotStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RollingRateSlotStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RollingRateSlotStartsInPlanned();
        RollingRateSlotMovesToScheduled();
        RollingRateSlotRejectsMovesFromCancelled();
    }
}
