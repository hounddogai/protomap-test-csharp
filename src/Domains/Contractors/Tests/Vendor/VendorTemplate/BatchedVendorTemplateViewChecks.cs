namespace Acme.Workforce.Contractors.Tests.Vendor.VendorTemplate;

using Acme.Workforce.Contractors.Vendor.VendorTemplate;

/// <summary>Checks of vendor template code, run by the project's runner.</summary>
public static class BatchedVendorTemplateViewChecks
{
    public static void BatchedVendorTemplateViewShowsTheStatusInCapitals()
    {
        var item = BatchedVendorTemplate.NewBatchedVendorTemplate("code-423", "title-443");
        var view = BatchedVendorTemplateView.ToBatchedVendorTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedVendorTemplateView.ToBatchedVendorTemplateViews(new List<BatchedVendorTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedVendorTemplateViewShowsTheStatusInCapitals();
    }
}
