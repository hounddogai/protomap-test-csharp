namespace Acme.Workforce.Orgdesign.Tests.Span.Digest;

using Acme.Workforce.Orgdesign.Span.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class SpanLedgerChecks
{
    public static void SpanLedgerStartsInCreated()
    {
        var item = SpanLedger.NewSpanLedger("code-459", "name-31");
        Check.Equal(item.Status, SpanLedgerStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanLedgerMovesToValidated()
    {
        var item = SpanLedger.NewSpanLedger("code-459", "name-31");
        try
        {
            item.MoveTo(SpanLedgerStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanLedgerStatus.Validated, "status");
    }

    public static void SpanLedgerRejectsMovesFromRetired()
    {
        var item = SpanLedger.NewSpanLedger("code-459", "name-31");
        item.Status = SpanLedgerStatus.Retired;
        Check.Fails(() => item.MoveTo(SpanLedgerStatus.Created), "move");
    }

    public static void RunAll()
    {
        SpanLedgerStartsInCreated();
        SpanLedgerMovesToValidated();
        SpanLedgerRejectsMovesFromRetired();
    }
}
