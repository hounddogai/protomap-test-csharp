namespace Acme.Workforce.Contractors.Tests.Engagement.Manifest;

using Acme.Workforce.Contractors.Engagement.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class EngagementReservationChecks
{
    public static void EngagementReservationStartsInRequested()
    {
        var item = EngagementReservation.NewEngagementReservation("code-761", "title-287");
        Check.Equal(item.Status, EngagementReservationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementReservationMovesToRevoked()
    {
        var item = EngagementReservation.NewEngagementReservation("code-761", "title-287");
        try
        {
            item.MoveTo(EngagementReservationStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementReservationStatus.Revoked, "status");
    }

    public static void EngagementReservationRejectsMovesFromExpired()
    {
        var item = EngagementReservation.NewEngagementReservation("code-761", "title-287");
        item.Status = EngagementReservationStatus.Expired;
        Check.Fails(() => item.MoveTo(EngagementReservationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        EngagementReservationStartsInRequested();
        EngagementReservationMovesToRevoked();
        EngagementReservationRejectsMovesFromExpired();
    }
}
