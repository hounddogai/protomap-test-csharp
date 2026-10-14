namespace Acme.Workforce.Contractors.Tests.Vendor.Batch;

using Acme.Workforce.Contractors.Vendor.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class VendorBatchChecks
{
    public static void VendorBatchStartsInNew()
    {
        var item = VendorBatch.NewVendorBatch("code-257", "label-849");
        Check.Equal(item.Status, VendorBatchStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorBatchMovesToRunning()
    {
        var item = VendorBatch.NewVendorBatch("code-257", "label-849");
        try
        {
            item.MoveTo(VendorBatchStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorBatchStatus.Running, "status");
    }

    public static void VendorBatchRejectsMovesFromFailed()
    {
        var item = VendorBatch.NewVendorBatch("code-257", "label-849");
        item.Status = VendorBatchStatus.Failed;
        Check.Fails(() => item.MoveTo(VendorBatchStatus.New), "move");
    }

    public static void RunAll()
    {
        VendorBatchStartsInNew();
        VendorBatchMovesToRunning();
        VendorBatchRejectsMovesFromFailed();
    }
}
