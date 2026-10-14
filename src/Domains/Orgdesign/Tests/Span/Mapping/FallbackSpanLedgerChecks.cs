namespace Acme.Workforce.Orgdesign.Tests.Span.Mapping;

using Acme.Workforce.Orgdesign.Span.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class FallbackSpanLedgerChecks
{
    public static void FallbackSpanLedgerStartsInPlanned()
    {
        var item = FallbackSpanLedger.NewFallbackSpanLedger("code-268", "name-932");
        Check.Equal(item.Status, FallbackSpanLedgerStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackSpanLedgerMovesToScheduled()
    {
        var item = FallbackSpanLedger.NewFallbackSpanLedger("code-268", "name-932");
        try
        {
            item.MoveTo(FallbackSpanLedgerStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackSpanLedgerStatus.Scheduled, "status");
    }

    public static void FallbackSpanLedgerRejectsMovesFromCancelled()
    {
        var item = FallbackSpanLedger.NewFallbackSpanLedger("code-268", "name-932");
        item.Status = FallbackSpanLedgerStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FallbackSpanLedgerStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FallbackSpanLedgerStartsInPlanned();
        FallbackSpanLedgerMovesToScheduled();
        FallbackSpanLedgerRejectsMovesFromCancelled();
    }
}
