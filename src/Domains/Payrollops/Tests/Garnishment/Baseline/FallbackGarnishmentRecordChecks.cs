namespace Acme.Workforce.Payrollops.Tests.Garnishment.Baseline;

using Acme.Workforce.Payrollops.Garnishment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackGarnishmentRecordChecks
{
    public static void FallbackGarnishmentRecordStartsInDraft()
    {
        var item = FallbackGarnishmentRecord.NewFallbackGarnishmentRecord("code-227", "title-819");
        Check.Equal(item.Status, FallbackGarnishmentRecordStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackGarnishmentRecordMovesToSuspended()
    {
        var item = FallbackGarnishmentRecord.NewFallbackGarnishmentRecord("code-227", "title-819");
        try
        {
            item.MoveTo(FallbackGarnishmentRecordStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackGarnishmentRecordStatus.Suspended, "status");
    }

    public static void FallbackGarnishmentRecordRejectsMovesFromClosed()
    {
        var item = FallbackGarnishmentRecord.NewFallbackGarnishmentRecord("code-227", "title-819");
        item.Status = FallbackGarnishmentRecordStatus.Closed;
        Check.Fails(() => item.MoveTo(FallbackGarnishmentRecordStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FallbackGarnishmentRecordStartsInDraft();
        FallbackGarnishmentRecordMovesToSuspended();
        FallbackGarnishmentRecordRejectsMovesFromClosed();
    }
}
