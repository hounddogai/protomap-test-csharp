namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationSchedule;

using Acme.Workforce.Compliance.Attestation.AttestationSchedule;

/// <summary>Checks of attestation schedule code, run by the project's runner.</summary>
public static class WeightedAttestationNoticeChecks
{
    public static void WeightedAttestationNoticeStartsInProposed()
    {
        var item = WeightedAttestationNotice.NewWeightedAttestationNotice("code-815", "name-422");
        Check.Equal(item.Status, WeightedAttestationNoticeStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedAttestationNoticeMovesToRejected()
    {
        var item = WeightedAttestationNotice.NewWeightedAttestationNotice("code-815", "name-422");
        try
        {
            item.MoveTo(WeightedAttestationNoticeStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedAttestationNoticeStatus.Rejected, "status");
    }

    public static void WeightedAttestationNoticeRejectsMovesFromWithdrawn()
    {
        var item = WeightedAttestationNotice.NewWeightedAttestationNotice("code-815", "name-422");
        item.Status = WeightedAttestationNoticeStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedAttestationNoticeStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedAttestationNoticeStartsInProposed();
        WeightedAttestationNoticeMovesToRejected();
        WeightedAttestationNoticeRejectsMovesFromWithdrawn();
    }
}
