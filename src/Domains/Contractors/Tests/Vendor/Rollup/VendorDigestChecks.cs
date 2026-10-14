namespace Acme.Workforce.Contractors.Tests.Vendor.Rollup;

using Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class VendorDigestChecks
{
    public static void VendorDigestStartsInNew()
    {
        var item = VendorDigest.NewVendorDigest("code-744", "name-942");
        Check.Equal(item.Status, VendorDigestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorDigestMovesToRunning()
    {
        var item = VendorDigest.NewVendorDigest("code-744", "name-942");
        try
        {
            item.MoveTo(VendorDigestStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorDigestStatus.Running, "status");
    }

    public static void VendorDigestRejectsMovesFromFailed()
    {
        var item = VendorDigest.NewVendorDigest("code-744", "name-942");
        item.Status = VendorDigestStatus.Failed;
        Check.Fails(() => item.MoveTo(VendorDigestStatus.New), "move");
    }

    public static void RunAll()
    {
        VendorDigestStartsInNew();
        VendorDigestMovesToRunning();
        VendorDigestRejectsMovesFromFailed();
    }
}
