namespace Acme.Workforce.Contractors.Tests.Vendor.Entry;

using Acme.Workforce.Contractors.Vendor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class LegacyVendorBatchChecks
{
    public static void LegacyVendorBatchStartsInNew()
    {
        var item = LegacyVendorBatch.NewLegacyVendorBatch("code-800", "title-291");
        Check.Equal(item.Status, LegacyVendorBatchStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyVendorBatchMovesToDone()
    {
        var item = LegacyVendorBatch.NewLegacyVendorBatch("code-800", "title-291");
        try
        {
            item.MoveTo(LegacyVendorBatchStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyVendorBatchStatus.Done, "status");
    }

    public static void LegacyVendorBatchRejectsMovesFromFailed()
    {
        var item = LegacyVendorBatch.NewLegacyVendorBatch("code-800", "title-291");
        item.Status = LegacyVendorBatchStatus.Failed;
        Check.Fails(() => item.MoveTo(LegacyVendorBatchStatus.New), "move");
    }

    public static void RunAll()
    {
        LegacyVendorBatchStartsInNew();
        LegacyVendorBatchMovesToDone();
        LegacyVendorBatchRejectsMovesFromFailed();
    }
}
