namespace Acme.Workforce.Compensation.Tests.Adjustment.Rollup;

using Acme.Workforce.Compensation.Adjustment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RegionalAdjustmentReservationChecks
{
    public static void RegionalAdjustmentReservationStartsInIdle()
    {
        var item = RegionalAdjustmentReservation.NewRegionalAdjustmentReservation("code-85", "title-363");
        Check.Equal(item.Status, RegionalAdjustmentReservationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalAdjustmentReservationMovesToWarming()
    {
        var item = RegionalAdjustmentReservation.NewRegionalAdjustmentReservation("code-85", "title-363");
        try
        {
            item.MoveTo(RegionalAdjustmentReservationStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalAdjustmentReservationStatus.Warming, "status");
    }

    public static void RegionalAdjustmentReservationRejectsMovesFromStopped()
    {
        var item = RegionalAdjustmentReservation.NewRegionalAdjustmentReservation("code-85", "title-363");
        item.Status = RegionalAdjustmentReservationStatus.Stopped;
        Check.Fails(() => item.MoveTo(RegionalAdjustmentReservationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RegionalAdjustmentReservationStartsInIdle();
        RegionalAdjustmentReservationMovesToWarming();
        RegionalAdjustmentReservationRejectsMovesFromStopped();
    }
}
