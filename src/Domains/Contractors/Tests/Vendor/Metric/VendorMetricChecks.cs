namespace Acme.Workforce.Contractors.Tests.Vendor.Metric;

using Acme.Workforce.Contractors.Vendor.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class VendorMetricChecks
{
    public static void VendorMetricStartsInRequested()
    {
        var item = VendorMetric.NewVendorMetric("code-305", "label-944");
        Check.Equal(item.Status, VendorMetricStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorMetricMovesToRevoked()
    {
        var item = VendorMetric.NewVendorMetric("code-305", "label-944");
        try
        {
            item.MoveTo(VendorMetricStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorMetricStatus.Revoked, "status");
    }

    public static void VendorMetricRejectsMovesFromExpired()
    {
        var item = VendorMetric.NewVendorMetric("code-305", "label-944");
        item.Status = VendorMetricStatus.Expired;
        Check.Fails(() => item.MoveTo(VendorMetricStatus.Requested), "move");
    }

    public static void RunAll()
    {
        VendorMetricStartsInRequested();
        VendorMetricMovesToRevoked();
        VendorMetricRejectsMovesFromExpired();
    }
}
