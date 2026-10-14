namespace Acme.Workforce.Workplace.Tests.AccessBadge.Threshold;

using Acme.Workforce.Workplace.AccessBadge.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccessBadgeReservationChecks
{
    public static void AccessBadgeReservationStartsInIdle()
    {
        var item = AccessBadgeReservation.NewAccessBadgeReservation("code-917", "label-786");
        Check.Equal(item.Status, AccessBadgeReservationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeReservationMovesToWarming()
    {
        var item = AccessBadgeReservation.NewAccessBadgeReservation("code-917", "label-786");
        try
        {
            item.MoveTo(AccessBadgeReservationStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeReservationStatus.Warming, "status");
    }

    public static void AccessBadgeReservationRejectsMovesFromStopped()
    {
        var item = AccessBadgeReservation.NewAccessBadgeReservation("code-917", "label-786");
        item.Status = AccessBadgeReservationStatus.Stopped;
        Check.Fails(() => item.MoveTo(AccessBadgeReservationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AccessBadgeReservationStartsInIdle();
        AccessBadgeReservationMovesToWarming();
        AccessBadgeReservationRejectsMovesFromStopped();
    }
}
