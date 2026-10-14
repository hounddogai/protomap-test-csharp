namespace Acme.Workforce.Payrollops.Tests.TaxForm.Exception;

using Acme.Workforce.Payrollops.TaxForm.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class BatchedTaxFormSegmentChecks
{
    public static void BatchedTaxFormSegmentStartsInRequested()
    {
        var item = BatchedTaxFormSegment.NewBatchedTaxFormSegment("code-574", "label-392");
        Check.Equal(item.Status, BatchedTaxFormSegmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedTaxFormSegmentMovesToGranted()
    {
        var item = BatchedTaxFormSegment.NewBatchedTaxFormSegment("code-574", "label-392");
        try
        {
            item.MoveTo(BatchedTaxFormSegmentStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedTaxFormSegmentStatus.Granted, "status");
    }

    public static void BatchedTaxFormSegmentRejectsMovesFromExpired()
    {
        var item = BatchedTaxFormSegment.NewBatchedTaxFormSegment("code-574", "label-392");
        item.Status = BatchedTaxFormSegmentStatus.Expired;
        Check.Fails(() => item.MoveTo(BatchedTaxFormSegmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BatchedTaxFormSegmentStartsInRequested();
        BatchedTaxFormSegmentMovesToGranted();
        BatchedTaxFormSegmentRejectsMovesFromExpired();
    }
}
