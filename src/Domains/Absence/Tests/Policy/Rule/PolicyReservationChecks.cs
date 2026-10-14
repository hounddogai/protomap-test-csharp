namespace Acme.Workforce.Absence.Tests.Policy.Rule;

using Acme.Workforce.Absence.Policy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PolicyReservationChecks
{
    public static void PolicyReservationStartsInProposed()
    {
        var item = PolicyReservation.NewPolicyReservation("code-261", "name-627");
        Check.Equal(item.Status, PolicyReservationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyReservationMovesToRejected()
    {
        var item = PolicyReservation.NewPolicyReservation("code-261", "name-627");
        try
        {
            item.MoveTo(PolicyReservationStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyReservationStatus.Rejected, "status");
    }

    public static void PolicyReservationRejectsMovesFromWithdrawn()
    {
        var item = PolicyReservation.NewPolicyReservation("code-261", "name-627");
        item.Status = PolicyReservationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PolicyReservationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PolicyReservationStartsInProposed();
        PolicyReservationMovesToRejected();
        PolicyReservationRejectsMovesFromWithdrawn();
    }
}
