namespace Acme.Workforce.Payrollops.Tests.TaxForm.Estimate;

using Acme.Workforce.Payrollops.TaxForm.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AdaptiveTaxFormRecordChecks
{
    public static void AdaptiveTaxFormRecordStartsInPlanned()
    {
        var item = AdaptiveTaxFormRecord.NewAdaptiveTaxFormRecord("code-852", "name-527");
        Check.Equal(item.Status, AdaptiveTaxFormRecordStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveTaxFormRecordMovesToCompleted()
    {
        var item = AdaptiveTaxFormRecord.NewAdaptiveTaxFormRecord("code-852", "name-527");
        try
        {
            item.MoveTo(AdaptiveTaxFormRecordStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveTaxFormRecordStatus.Completed, "status");
    }

    public static void AdaptiveTaxFormRecordRejectsMovesFromCancelled()
    {
        var item = AdaptiveTaxFormRecord.NewAdaptiveTaxFormRecord("code-852", "name-527");
        item.Status = AdaptiveTaxFormRecordStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdaptiveTaxFormRecordStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdaptiveTaxFormRecordStartsInPlanned();
        AdaptiveTaxFormRecordMovesToCompleted();
        AdaptiveTaxFormRecordRejectsMovesFromCancelled();
    }
}
