namespace Acme.Workforce.Contractors.Tests.Vendor.VendorTemplate;

using Acme.Workforce.Contractors.Vendor.VendorTemplate;

/// <summary>Checks of vendor template code, run by the project's runner.</summary>
public static class PendingVendorManifestChecks
{
    public static void PendingVendorManifestStartsInPlanned()
    {
        var item = PendingVendorManifest.NewPendingVendorManifest("code-304", "label-138");
        Check.Equal(item.Status, PendingVendorManifestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingVendorManifestMovesToCancelled()
    {
        var item = PendingVendorManifest.NewPendingVendorManifest("code-304", "label-138");
        try
        {
            item.MoveTo(PendingVendorManifestStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingVendorManifestStatus.Cancelled, "status");
    }

    public static void PendingVendorManifestRejectsMovesFromCancelled()
    {
        var item = PendingVendorManifest.NewPendingVendorManifest("code-304", "label-138");
        item.Status = PendingVendorManifestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PendingVendorManifestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PendingVendorManifestStartsInPlanned();
        PendingVendorManifestMovesToCancelled();
        PendingVendorManifestRejectsMovesFromCancelled();
    }
}
