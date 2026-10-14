namespace Acme.Workforce.Orgdesign.Tests.Span.Preference;

using Acme.Workforce.Orgdesign.Span.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LenientSpanLedgerChecks
{
    public static void LenientSpanLedgerStartsInProposed()
    {
        var item = LenientSpanLedger.NewLenientSpanLedger("code-158", "label-451");
        Check.Equal(item.Status, LenientSpanLedgerStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientSpanLedgerMovesToApproved()
    {
        var item = LenientSpanLedger.NewLenientSpanLedger("code-158", "label-451");
        try
        {
            item.MoveTo(LenientSpanLedgerStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientSpanLedgerStatus.Approved, "status");
    }

    public static void LenientSpanLedgerRejectsMovesFromWithdrawn()
    {
        var item = LenientSpanLedger.NewLenientSpanLedger("code-158", "label-451");
        item.Status = LenientSpanLedgerStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LenientSpanLedgerStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LenientSpanLedgerStartsInProposed();
        LenientSpanLedgerMovesToApproved();
        LenientSpanLedgerRejectsMovesFromWithdrawn();
    }
}
