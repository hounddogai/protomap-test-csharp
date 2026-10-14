namespace Acme.Workforce.Contractors.Tests.Vendor.Batch;

using Acme.Workforce.Contractors.Vendor.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class VendorBatchViewChecks
{
    public static void VendorBatchViewShowsTheStatusInCapitals()
    {
        var item = VendorBatch.NewVendorBatch("code-257", "label-849");
        var view = VendorBatchView.ToVendorBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VendorBatchView.ToVendorBatchViews(new List<VendorBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VendorBatchViewShowsTheStatusInCapitals();
    }
}
