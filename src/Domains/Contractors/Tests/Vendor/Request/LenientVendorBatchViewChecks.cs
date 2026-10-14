namespace Acme.Workforce.Contractors.Tests.Vendor.Request;

using Acme.Workforce.Contractors.Vendor.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LenientVendorBatchViewChecks
{
    public static void LenientVendorBatchViewShowsTheStatusInCapitals()
    {
        var item = LenientVendorBatch.NewLenientVendorBatch("code-431", "label-41");
        var view = LenientVendorBatchView.ToLenientVendorBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientVendorBatchView.ToLenientVendorBatchViews(new List<LenientVendorBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientVendorBatchViewShowsTheStatusInCapitals();
    }
}
