namespace Acme.Workforce.Contractors.Tests.Vendor.Request;

using Acme.Workforce.Contractors.Vendor.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LenientVendorBatchChecks
{
    public static void LenientVendorBatchStartsInNew()
    {
        var item = LenientVendorBatch.NewLenientVendorBatch("code-431", "label-41");
        Check.Equal(item.Status, LenientVendorBatchStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientVendorBatchMovesToQueued()
    {
        var item = LenientVendorBatch.NewLenientVendorBatch("code-431", "label-41");
        try
        {
            item.MoveTo(LenientVendorBatchStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientVendorBatchStatus.Queued, "status");
    }

    public static void LenientVendorBatchRejectsMovesFromFailed()
    {
        var item = LenientVendorBatch.NewLenientVendorBatch("code-431", "label-41");
        item.Status = LenientVendorBatchStatus.Failed;
        Check.Fails(() => item.MoveTo(LenientVendorBatchStatus.New), "move");
    }

    public static void RunAll()
    {
        LenientVendorBatchStartsInNew();
        LenientVendorBatchMovesToQueued();
        LenientVendorBatchRejectsMovesFromFailed();
    }
}
