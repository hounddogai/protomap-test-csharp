namespace Acme.Workforce.Workplace.Tests.Booking.BookingVariance;

using Acme.Workforce.Workplace.Booking.BookingVariance;

/// <summary>Checks of booking variance code, run by the project's runner.</summary>
public static class LegacyBookingAuditChecks
{
    public static void LegacyBookingAuditStartsInDraft()
    {
        var item = LegacyBookingAudit.NewLegacyBookingAudit("code-847", "title-109");
        Check.Equal(item.Status, LegacyBookingAuditStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyBookingAuditMovesToPending()
    {
        var item = LegacyBookingAudit.NewLegacyBookingAudit("code-847", "title-109");
        try
        {
            item.MoveTo(LegacyBookingAuditStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyBookingAuditStatus.Pending, "status");
    }

    public static void LegacyBookingAuditRejectsMovesFromClosed()
    {
        var item = LegacyBookingAudit.NewLegacyBookingAudit("code-847", "title-109");
        item.Status = LegacyBookingAuditStatus.Closed;
        Check.Fails(() => item.MoveTo(LegacyBookingAuditStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LegacyBookingAuditStartsInDraft();
        LegacyBookingAuditMovesToPending();
        LegacyBookingAuditRejectsMovesFromClosed();
    }
}
