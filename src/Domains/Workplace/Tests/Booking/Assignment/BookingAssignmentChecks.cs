namespace Acme.Workforce.Workplace.Tests.Booking.Assignment;

using Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class BookingAssignmentChecks
{
    public static void BookingAssignmentStartsInProposed()
    {
        var item = BookingAssignment.NewBookingAssignment("code-359", "name-538");
        Check.Equal(item.Status, BookingAssignmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingAssignmentMovesToApproved()
    {
        var item = BookingAssignment.NewBookingAssignment("code-359", "name-538");
        try
        {
            item.MoveTo(BookingAssignmentStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingAssignmentStatus.Approved, "status");
    }

    public static void BookingAssignmentRejectsMovesFromWithdrawn()
    {
        var item = BookingAssignment.NewBookingAssignment("code-359", "name-538");
        item.Status = BookingAssignmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BookingAssignmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BookingAssignmentStartsInProposed();
        BookingAssignmentMovesToApproved();
        BookingAssignmentRejectsMovesFromWithdrawn();
    }
}
