namespace Acme.Workforce.Contractors.Tests.Vendor.VendorMetric;

using Acme.Workforce.Contractors.Vendor.VendorMetric;

/// <summary>Checks of vendor metric code, run by the project's runner.</summary>
public static class VendorWindowChecks
{
    public static void VendorWindowStartsInPlanned()
    {
        var item = VendorWindow.NewVendorWindow("code-56", "label-667");
        Check.Equal(item.Status, VendorWindowStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorWindowMovesToScheduled()
    {
        var item = VendorWindow.NewVendorWindow("code-56", "label-667");
        try
        {
            item.MoveTo(VendorWindowStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorWindowStatus.Scheduled, "status");
    }

    public static void VendorWindowRejectsMovesFromCancelled()
    {
        var item = VendorWindow.NewVendorWindow("code-56", "label-667");
        item.Status = VendorWindowStatus.Cancelled;
        Check.Fails(() => item.MoveTo(VendorWindowStatus.Planned), "move");
    }

    public static void RunAll()
    {
        VendorWindowStartsInPlanned();
        VendorWindowMovesToScheduled();
        VendorWindowRejectsMovesFromCancelled();
    }
}
