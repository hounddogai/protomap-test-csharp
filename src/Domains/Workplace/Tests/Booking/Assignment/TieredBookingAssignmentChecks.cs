namespace Acme.Workforce.Workplace.Tests.Booking.Assignment;

using Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TieredBookingAssignmentChecks
{
    public static void TieredBookingAssignmentStartsInRequested()
    {
        var item = TieredBookingAssignment.NewTieredBookingAssignment("code-308", "label-908");
        Check.Equal(item.Status, TieredBookingAssignmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredBookingAssignmentMovesToGranted()
    {
        var item = TieredBookingAssignment.NewTieredBookingAssignment("code-308", "label-908");
        try
        {
            item.MoveTo(TieredBookingAssignmentStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredBookingAssignmentStatus.Granted, "status");
    }

    public static void TieredBookingAssignmentRejectsMovesFromExpired()
    {
        var item = TieredBookingAssignment.NewTieredBookingAssignment("code-308", "label-908");
        item.Status = TieredBookingAssignmentStatus.Expired;
        Check.Fails(() => item.MoveTo(TieredBookingAssignmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TieredBookingAssignmentStartsInRequested();
        TieredBookingAssignmentMovesToGranted();
        TieredBookingAssignmentRejectsMovesFromExpired();
    }
}
