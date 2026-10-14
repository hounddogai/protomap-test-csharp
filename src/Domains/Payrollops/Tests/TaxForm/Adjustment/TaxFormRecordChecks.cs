namespace Acme.Workforce.Payrollops.Tests.TaxForm.Adjustment;

using Acme.Workforce.Payrollops.TaxForm.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TaxFormRecordChecks
{
    public static void TaxFormRecordStartsInPlanned()
    {
        var item = TaxFormRecord.NewTaxFormRecord("code-294", "label-581");
        Check.Equal(item.Status, TaxFormRecordStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormRecordMovesToInProgress()
    {
        var item = TaxFormRecord.NewTaxFormRecord("code-294", "label-581");
        try
        {
            item.MoveTo(TaxFormRecordStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormRecordStatus.InProgress, "status");
    }

    public static void TaxFormRecordRejectsMovesFromCancelled()
    {
        var item = TaxFormRecord.NewTaxFormRecord("code-294", "label-581");
        item.Status = TaxFormRecordStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TaxFormRecordStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TaxFormRecordStartsInPlanned();
        TaxFormRecordMovesToInProgress();
        TaxFormRecordRejectsMovesFromCancelled();
    }
}
