namespace Acme.Workforce.Compensation.Tests.Adjustment.Schedule;

using Acme.Workforce.Compensation.Adjustment.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ScheduledAdjustmentReservationChecks
{
    public static void ScheduledAdjustmentReservationStartsInRequested()
    {
        var item = ScheduledAdjustmentReservation.NewScheduledAdjustmentReservation("code-651", "title-532");
        Check.Equal(item.Status, ScheduledAdjustmentReservationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledAdjustmentReservationMovesToRevoked()
    {
        var item = ScheduledAdjustmentReservation.NewScheduledAdjustmentReservation("code-651", "title-532");
        try
        {
            item.MoveTo(ScheduledAdjustmentReservationStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledAdjustmentReservationStatus.Revoked, "status");
    }

    public static void ScheduledAdjustmentReservationRejectsMovesFromExpired()
    {
        var item = ScheduledAdjustmentReservation.NewScheduledAdjustmentReservation("code-651", "title-532");
        item.Status = ScheduledAdjustmentReservationStatus.Expired;
        Check.Fails(() => item.MoveTo(ScheduledAdjustmentReservationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ScheduledAdjustmentReservationStartsInRequested();
        ScheduledAdjustmentReservationMovesToRevoked();
        ScheduledAdjustmentReservationRejectsMovesFromExpired();
    }
}
