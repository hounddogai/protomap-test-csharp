namespace Acme.Workforce.Compliance.Tests.PolicyAck.Reservation;

using Acme.Workforce.Compliance.PolicyAck.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class PolicyAckReservationChecks
{
    public static void PolicyAckReservationStartsInOpen()
    {
        var item = PolicyAckReservation.NewPolicyAckReservation("code-930", "title-430");
        Check.Equal(item.Status, PolicyAckReservationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckReservationMovesToReopened()
    {
        var item = PolicyAckReservation.NewPolicyAckReservation("code-930", "title-430");
        try
        {
            item.MoveTo(PolicyAckReservationStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckReservationStatus.Reopened, "status");
    }

    public static void PolicyAckReservationRejectsMovesFromArchived()
    {
        var item = PolicyAckReservation.NewPolicyAckReservation("code-930", "title-430");
        item.Status = PolicyAckReservationStatus.Archived;
        Check.Fails(() => item.MoveTo(PolicyAckReservationStatus.Open), "move");
    }

    public static void RunAll()
    {
        PolicyAckReservationStartsInOpen();
        PolicyAckReservationMovesToReopened();
        PolicyAckReservationRejectsMovesFromArchived();
    }
}
