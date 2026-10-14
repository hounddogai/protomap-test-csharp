namespace Acme.Workforce.Workplace.Tests.Booking.BookingReview;

using Acme.Workforce.Workplace.Booking.BookingReview;

/// <summary>Checks of booking review code, run by the project's runner.</summary>
public static class ManualBookingAssignmentChecks
{
    public static void ManualBookingAssignmentStartsInOpen()
    {
        var item = ManualBookingAssignment.NewManualBookingAssignment("code-835", "name-592");
        Check.Equal(item.Status, ManualBookingAssignmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualBookingAssignmentMovesToReopened()
    {
        var item = ManualBookingAssignment.NewManualBookingAssignment("code-835", "name-592");
        try
        {
            item.MoveTo(ManualBookingAssignmentStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualBookingAssignmentStatus.Reopened, "status");
    }

    public static void ManualBookingAssignmentRejectsMovesFromArchived()
    {
        var item = ManualBookingAssignment.NewManualBookingAssignment("code-835", "name-592");
        item.Status = ManualBookingAssignmentStatus.Archived;
        Check.Fails(() => item.MoveTo(ManualBookingAssignmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        ManualBookingAssignmentStartsInOpen();
        ManualBookingAssignmentMovesToReopened();
        ManualBookingAssignmentRejectsMovesFromArchived();
    }
}
