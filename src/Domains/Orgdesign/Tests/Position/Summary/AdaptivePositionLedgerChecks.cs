namespace Acme.Workforce.Orgdesign.Tests.Position.Summary;

using Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AdaptivePositionLedgerChecks
{
    public static void AdaptivePositionLedgerStartsInProposed()
    {
        var item = AdaptivePositionLedger.NewAdaptivePositionLedger("code-622", "title-519");
        Check.Equal(item.Status, AdaptivePositionLedgerStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptivePositionLedgerMovesToRejected()
    {
        var item = AdaptivePositionLedger.NewAdaptivePositionLedger("code-622", "title-519");
        try
        {
            item.MoveTo(AdaptivePositionLedgerStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptivePositionLedgerStatus.Rejected, "status");
    }

    public static void AdaptivePositionLedgerRejectsMovesFromWithdrawn()
    {
        var item = AdaptivePositionLedger.NewAdaptivePositionLedger("code-622", "title-519");
        item.Status = AdaptivePositionLedgerStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AdaptivePositionLedgerStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AdaptivePositionLedgerStartsInProposed();
        AdaptivePositionLedgerMovesToRejected();
        AdaptivePositionLedgerRejectsMovesFromWithdrawn();
    }
}
