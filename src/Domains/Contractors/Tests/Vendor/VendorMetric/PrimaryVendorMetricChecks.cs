namespace Acme.Workforce.Contractors.Tests.Vendor.VendorMetric;

using Acme.Workforce.Contractors.Vendor.VendorMetric;

/// <summary>Checks of vendor metric code, run by the project's runner.</summary>
public static class PrimaryVendorMetricChecks
{
    public static void PrimaryVendorMetricStartsInOpen()
    {
        var item = PrimaryVendorMetric.NewPrimaryVendorMetric("code-62", "label-340");
        Check.Equal(item.Status, PrimaryVendorMetricStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryVendorMetricMovesToInReview()
    {
        var item = PrimaryVendorMetric.NewPrimaryVendorMetric("code-62", "label-340");
        try
        {
            item.MoveTo(PrimaryVendorMetricStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryVendorMetricStatus.InReview, "status");
    }

    public static void PrimaryVendorMetricRejectsMovesFromArchived()
    {
        var item = PrimaryVendorMetric.NewPrimaryVendorMetric("code-62", "label-340");
        item.Status = PrimaryVendorMetricStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryVendorMetricStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryVendorMetricStartsInOpen();
        PrimaryVendorMetricMovesToInReview();
        PrimaryVendorMetricRejectsMovesFromArchived();
    }
}
