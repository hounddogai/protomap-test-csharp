namespace Acme.Workforce.Compliance.Tests.Attestation.RecordItem;

using Acme.Workforce.Compliance.Attestation.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DefaultAttestationRollupChecks
{
    public static void DefaultAttestationRollupStartsInProposed()
    {
        var item = DefaultAttestationRollup.NewDefaultAttestationRollup("code-914", "label-874");
        Check.Equal(item.Status, DefaultAttestationRollupStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultAttestationRollupMovesToWithdrawn()
    {
        var item = DefaultAttestationRollup.NewDefaultAttestationRollup("code-914", "label-874");
        try
        {
            item.MoveTo(DefaultAttestationRollupStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultAttestationRollupStatus.Withdrawn, "status");
    }

    public static void DefaultAttestationRollupRejectsMovesFromWithdrawn()
    {
        var item = DefaultAttestationRollup.NewDefaultAttestationRollup("code-914", "label-874");
        item.Status = DefaultAttestationRollupStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DefaultAttestationRollupStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DefaultAttestationRollupStartsInProposed();
        DefaultAttestationRollupMovesToWithdrawn();
        DefaultAttestationRollupRejectsMovesFromWithdrawn();
    }
}
