namespace Acme.Workforce.Contractors.Tests.Engagement.Ledger;

using Acme.Workforce.Contractors.Engagement.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FallbackEngagementLedgerChecks
{
    public static void FallbackEngagementLedgerStartsInCreated()
    {
        var item = FallbackEngagementLedger.NewFallbackEngagementLedger("code-409", "title-487");
        Check.Equal(item.Status, FallbackEngagementLedgerStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackEngagementLedgerMovesToValidated()
    {
        var item = FallbackEngagementLedger.NewFallbackEngagementLedger("code-409", "title-487");
        try
        {
            item.MoveTo(FallbackEngagementLedgerStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackEngagementLedgerStatus.Validated, "status");
    }

    public static void FallbackEngagementLedgerRejectsMovesFromRetired()
    {
        var item = FallbackEngagementLedger.NewFallbackEngagementLedger("code-409", "title-487");
        item.Status = FallbackEngagementLedgerStatus.Retired;
        Check.Fails(() => item.MoveTo(FallbackEngagementLedgerStatus.Created), "move");
    }

    public static void RunAll()
    {
        FallbackEngagementLedgerStartsInCreated();
        FallbackEngagementLedgerMovesToValidated();
        FallbackEngagementLedgerRejectsMovesFromRetired();
    }
}
