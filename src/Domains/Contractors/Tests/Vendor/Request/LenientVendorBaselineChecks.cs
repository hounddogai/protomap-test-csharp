namespace Acme.Workforce.Contractors.Tests.Vendor.Request;

using Acme.Workforce.Contractors.Vendor.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LenientVendorBaselineChecks
{
    public static void LenientVendorBaselineStartsInPlanned()
    {
        var item = LenientVendorBaseline.NewLenientVendorBaseline("code-225", "title-22");
        Check.Equal(item.Status, LenientVendorBaselineStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientVendorBaselineMovesToScheduled()
    {
        var item = LenientVendorBaseline.NewLenientVendorBaseline("code-225", "title-22");
        try
        {
            item.MoveTo(LenientVendorBaselineStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientVendorBaselineStatus.Scheduled, "status");
    }

    public static void LenientVendorBaselineRejectsMovesFromCancelled()
    {
        var item = LenientVendorBaseline.NewLenientVendorBaseline("code-225", "title-22");
        item.Status = LenientVendorBaselineStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientVendorBaselineStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientVendorBaselineStartsInPlanned();
        LenientVendorBaselineMovesToScheduled();
        LenientVendorBaselineRejectsMovesFromCancelled();
    }
}
