namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeBudget;

using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Checks of badge budget code, run by the project's runner.</summary>
public static class ScheduledAccessBadgeReservationChecks
{
    public static void ScheduledAccessBadgeReservationStartsInProposed()
    {
        var item = ScheduledAccessBadgeReservation.NewScheduledAccessBadgeReservation("code-235", "title-924");
        Check.Equal(item.Status, ScheduledAccessBadgeReservationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledAccessBadgeReservationMovesToRejected()
    {
        var item = ScheduledAccessBadgeReservation.NewScheduledAccessBadgeReservation("code-235", "title-924");
        try
        {
            item.MoveTo(ScheduledAccessBadgeReservationStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledAccessBadgeReservationStatus.Rejected, "status");
    }

    public static void ScheduledAccessBadgeReservationRejectsMovesFromWithdrawn()
    {
        var item = ScheduledAccessBadgeReservation.NewScheduledAccessBadgeReservation("code-235", "title-924");
        item.Status = ScheduledAccessBadgeReservationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledAccessBadgeReservationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledAccessBadgeReservationStartsInProposed();
        ScheduledAccessBadgeReservationMovesToRejected();
        ScheduledAccessBadgeReservationRejectsMovesFromWithdrawn();
    }
}
