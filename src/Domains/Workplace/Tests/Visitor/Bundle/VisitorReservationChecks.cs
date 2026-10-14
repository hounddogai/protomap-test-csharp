namespace Acme.Workforce.Workplace.Tests.Visitor.Bundle;

using Acme.Workforce.Workplace.Visitor.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class VisitorReservationChecks
{
    public static void VisitorReservationStartsInRequested()
    {
        var item = VisitorReservation.NewVisitorReservation("code-720", "title-944");
        Check.Equal(item.Status, VisitorReservationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorReservationMovesToRevoked()
    {
        var item = VisitorReservation.NewVisitorReservation("code-720", "title-944");
        try
        {
            item.MoveTo(VisitorReservationStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorReservationStatus.Revoked, "status");
    }

    public static void VisitorReservationRejectsMovesFromExpired()
    {
        var item = VisitorReservation.NewVisitorReservation("code-720", "title-944");
        item.Status = VisitorReservationStatus.Expired;
        Check.Fails(() => item.MoveTo(VisitorReservationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        VisitorReservationStartsInRequested();
        VisitorReservationMovesToRevoked();
        VisitorReservationRejectsMovesFromExpired();
    }
}
