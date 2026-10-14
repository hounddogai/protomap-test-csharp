namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckReservation;

using Acme.Workforce.Compliance.PolicyAck.AckReservation;

/// <summary>Checks of ack reservation code, run by the project's runner.</summary>
public static class AdaptivePolicyAckSnapshotChecks
{
    public static void AdaptivePolicyAckSnapshotStartsInCreated()
    {
        var item = AdaptivePolicyAckSnapshot.NewAdaptivePolicyAckSnapshot("code-163", "label-199");
        Check.Equal(item.Status, AdaptivePolicyAckSnapshotStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptivePolicyAckSnapshotMovesToPublished()
    {
        var item = AdaptivePolicyAckSnapshot.NewAdaptivePolicyAckSnapshot("code-163", "label-199");
        try
        {
            item.MoveTo(AdaptivePolicyAckSnapshotStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptivePolicyAckSnapshotStatus.Published, "status");
    }

    public static void AdaptivePolicyAckSnapshotRejectsMovesFromRetired()
    {
        var item = AdaptivePolicyAckSnapshot.NewAdaptivePolicyAckSnapshot("code-163", "label-199");
        item.Status = AdaptivePolicyAckSnapshotStatus.Retired;
        Check.Fails(() => item.MoveTo(AdaptivePolicyAckSnapshotStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdaptivePolicyAckSnapshotStartsInCreated();
        AdaptivePolicyAckSnapshotMovesToPublished();
        AdaptivePolicyAckSnapshotRejectsMovesFromRetired();
    }
}
