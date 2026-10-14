namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class ScheduledBookingTemplateChecks
{
    public static void ScheduledBookingTemplateStartsInProposed()
    {
        var item = ScheduledBookingTemplate.NewScheduledBookingTemplate("code-651", "name-222");
        Check.Equal(item.Status, ScheduledBookingTemplateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledBookingTemplateMovesToApproved()
    {
        var item = ScheduledBookingTemplate.NewScheduledBookingTemplate("code-651", "name-222");
        try
        {
            item.MoveTo(ScheduledBookingTemplateStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledBookingTemplateStatus.Approved, "status");
    }

    public static void ScheduledBookingTemplateRejectsMovesFromWithdrawn()
    {
        var item = ScheduledBookingTemplate.NewScheduledBookingTemplate("code-651", "name-222");
        item.Status = ScheduledBookingTemplateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledBookingTemplateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledBookingTemplateStartsInProposed();
        ScheduledBookingTemplateMovesToApproved();
        ScheduledBookingTemplateRejectsMovesFromWithdrawn();
    }
}
