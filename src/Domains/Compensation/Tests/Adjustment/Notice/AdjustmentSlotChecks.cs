namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentSlotChecks
{
    public static void AdjustmentSlotStartsInIdle()
    {
        var item = AdjustmentSlot.NewAdjustmentSlot("code-783", "title-756");
        Check.Equal(item.Status, AdjustmentSlotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentSlotMovesToDraining()
    {
        var item = AdjustmentSlot.NewAdjustmentSlot("code-783", "title-756");
        try
        {
            item.MoveTo(AdjustmentSlotStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentSlotStatus.Draining, "status");
    }

    public static void AdjustmentSlotRejectsMovesFromStopped()
    {
        var item = AdjustmentSlot.NewAdjustmentSlot("code-783", "title-756");
        item.Status = AdjustmentSlotStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdjustmentSlotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdjustmentSlotStartsInIdle();
        AdjustmentSlotMovesToDraining();
        AdjustmentSlotRejectsMovesFromStopped();
    }
}
