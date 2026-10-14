namespace Acme.Workforce.Workplace.Tests.Booking.BookingPlan;

using Acme.Workforce.Workplace.Booking.BookingPlan;

/// <summary>Checks of booking plan code, run by the project's runner.</summary>
public static class ManualBookingAuditChecks
{
    public static void ManualBookingAuditStartsInProposed()
    {
        var item = ManualBookingAudit.NewManualBookingAudit("code-671", "title-429");
        Check.Equal(item.Status, ManualBookingAuditStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualBookingAuditMovesToRejected()
    {
        var item = ManualBookingAudit.NewManualBookingAudit("code-671", "title-429");
        try
        {
            item.MoveTo(ManualBookingAuditStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualBookingAuditStatus.Rejected, "status");
    }

    public static void ManualBookingAuditRejectsMovesFromWithdrawn()
    {
        var item = ManualBookingAudit.NewManualBookingAudit("code-671", "title-429");
        item.Status = ManualBookingAuditStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ManualBookingAuditStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ManualBookingAuditStartsInProposed();
        ManualBookingAuditMovesToRejected();
        ManualBookingAuditRejectsMovesFromWithdrawn();
    }
}
