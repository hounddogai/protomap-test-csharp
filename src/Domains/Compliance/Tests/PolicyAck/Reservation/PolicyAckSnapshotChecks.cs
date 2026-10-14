namespace Acme.Workforce.Compliance.Tests.PolicyAck.Reservation;

using Acme.Workforce.Compliance.PolicyAck.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class PolicyAckSnapshotChecks
{
    public static void PolicyAckSnapshotStartsInOpen()
    {
        var item = PolicyAckSnapshot.NewPolicyAckSnapshot("code-992", "label-106");
        Check.Equal(item.Status, PolicyAckSnapshotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckSnapshotMovesToResolved()
    {
        var item = PolicyAckSnapshot.NewPolicyAckSnapshot("code-992", "label-106");
        try
        {
            item.MoveTo(PolicyAckSnapshotStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckSnapshotStatus.Resolved, "status");
    }

    public static void PolicyAckSnapshotRejectsMovesFromArchived()
    {
        var item = PolicyAckSnapshot.NewPolicyAckSnapshot("code-992", "label-106");
        item.Status = PolicyAckSnapshotStatus.Archived;
        Check.Fails(() => item.MoveTo(PolicyAckSnapshotStatus.Open), "move");
    }

    public static void RunAll()
    {
        PolicyAckSnapshotStartsInOpen();
        PolicyAckSnapshotMovesToResolved();
        PolicyAckSnapshotRejectsMovesFromArchived();
    }
}
