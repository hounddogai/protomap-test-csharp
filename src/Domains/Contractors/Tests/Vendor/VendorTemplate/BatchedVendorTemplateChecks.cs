namespace Acme.Workforce.Contractors.Tests.Vendor.VendorTemplate;

using Acme.Workforce.Contractors.Vendor.VendorTemplate;

/// <summary>Checks of vendor template code, run by the project's runner.</summary>
public static class BatchedVendorTemplateChecks
{
    public static void BatchedVendorTemplateStartsInPlanned()
    {
        var item = BatchedVendorTemplate.NewBatchedVendorTemplate("code-423", "title-443");
        Check.Equal(item.Status, BatchedVendorTemplateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedVendorTemplateMovesToScheduled()
    {
        var item = BatchedVendorTemplate.NewBatchedVendorTemplate("code-423", "title-443");
        try
        {
            item.MoveTo(BatchedVendorTemplateStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedVendorTemplateStatus.Scheduled, "status");
    }

    public static void BatchedVendorTemplateRejectsMovesFromCancelled()
    {
        var item = BatchedVendorTemplate.NewBatchedVendorTemplate("code-423", "title-443");
        item.Status = BatchedVendorTemplateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BatchedVendorTemplateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BatchedVendorTemplateStartsInPlanned();
        BatchedVendorTemplateMovesToScheduled();
        BatchedVendorTemplateRejectsMovesFromCancelled();
    }
}
