namespace Acme.Workforce.Contractors.Tests.Vendor.VendorRequest;

using Acme.Workforce.Contractors.Vendor.VendorRequest;

/// <summary>Checks of vendor request code, run by the project's runner.</summary>
public static class RegionalVendorRequestChecks
{
    public static void RegionalVendorRequestStartsInIdle()
    {
        var item = RegionalVendorRequest.NewRegionalVendorRequest("code-372", "title-6");
        Check.Equal(item.Status, RegionalVendorRequestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalVendorRequestMovesToDraining()
    {
        var item = RegionalVendorRequest.NewRegionalVendorRequest("code-372", "title-6");
        try
        {
            item.MoveTo(RegionalVendorRequestStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalVendorRequestStatus.Draining, "status");
    }

    public static void RegionalVendorRequestRejectsMovesFromStopped()
    {
        var item = RegionalVendorRequest.NewRegionalVendorRequest("code-372", "title-6");
        item.Status = RegionalVendorRequestStatus.Stopped;
        Check.Fails(() => item.MoveTo(RegionalVendorRequestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RegionalVendorRequestStartsInIdle();
        RegionalVendorRequestMovesToDraining();
        RegionalVendorRequestRejectsMovesFromStopped();
    }
}
