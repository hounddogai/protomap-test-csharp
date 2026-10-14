namespace Acme.Workforce.Contractors.Tests.Vendor.Variance;

using Acme.Workforce.Contractors.Vendor.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class VendorVarianceChecks
{
    public static void VendorVarianceStartsInCreated()
    {
        var item = VendorVariance.NewVendorVariance("code-678", "title-206");
        Check.Equal(item.Status, VendorVarianceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorVarianceMovesToPublished()
    {
        var item = VendorVariance.NewVendorVariance("code-678", "title-206");
        try
        {
            item.MoveTo(VendorVarianceStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorVarianceStatus.Published, "status");
    }

    public static void VendorVarianceRejectsMovesFromRetired()
    {
        var item = VendorVariance.NewVendorVariance("code-678", "title-206");
        item.Status = VendorVarianceStatus.Retired;
        Check.Fails(() => item.MoveTo(VendorVarianceStatus.Created), "move");
    }

    public static void RunAll()
    {
        VendorVarianceStartsInCreated();
        VendorVarianceMovesToPublished();
        VendorVarianceRejectsMovesFromRetired();
    }
}
