namespace Acme.Workforce.Contractors.Tests.Vendor.Proposal;

using Acme.Workforce.Contractors.Vendor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DefaultVendorSegmentChecks
{
    public static void DefaultVendorSegmentStartsInIdle()
    {
        var item = DefaultVendorSegment.NewDefaultVendorSegment("code-861", "label-968");
        Check.Equal(item.Status, DefaultVendorSegmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultVendorSegmentMovesToWarming()
    {
        var item = DefaultVendorSegment.NewDefaultVendorSegment("code-861", "label-968");
        try
        {
            item.MoveTo(DefaultVendorSegmentStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultVendorSegmentStatus.Warming, "status");
    }

    public static void DefaultVendorSegmentRejectsMovesFromStopped()
    {
        var item = DefaultVendorSegment.NewDefaultVendorSegment("code-861", "label-968");
        item.Status = DefaultVendorSegmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(DefaultVendorSegmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DefaultVendorSegmentStartsInIdle();
        DefaultVendorSegmentMovesToWarming();
        DefaultVendorSegmentRejectsMovesFromStopped();
    }
}
