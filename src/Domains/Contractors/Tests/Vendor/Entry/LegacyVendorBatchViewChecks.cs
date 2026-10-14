namespace Acme.Workforce.Contractors.Tests.Vendor.Entry;

using Acme.Workforce.Contractors.Vendor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class LegacyVendorBatchViewChecks
{
    public static void LegacyVendorBatchViewShowsTheStatusInCapitals()
    {
        var item = LegacyVendorBatch.NewLegacyVendorBatch("code-800", "title-291");
        var view = LegacyVendorBatchView.ToLegacyVendorBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyVendorBatchView.ToLegacyVendorBatchViews(new List<LegacyVendorBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyVendorBatchViewShowsTheStatusInCapitals();
    }
}
