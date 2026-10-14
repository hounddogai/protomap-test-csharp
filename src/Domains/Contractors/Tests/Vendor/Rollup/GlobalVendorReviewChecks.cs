namespace Acme.Workforce.Contractors.Tests.Vendor.Rollup;

using Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class GlobalVendorReviewChecks
{
    public static void GlobalVendorReviewStartsInPlanned()
    {
        var item = GlobalVendorReview.NewGlobalVendorReview("code-830", "name-195");
        Check.Equal(item.Status, GlobalVendorReviewStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalVendorReviewMovesToScheduled()
    {
        var item = GlobalVendorReview.NewGlobalVendorReview("code-830", "name-195");
        try
        {
            item.MoveTo(GlobalVendorReviewStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalVendorReviewStatus.Scheduled, "status");
    }

    public static void GlobalVendorReviewRejectsMovesFromCancelled()
    {
        var item = GlobalVendorReview.NewGlobalVendorReview("code-830", "name-195");
        item.Status = GlobalVendorReviewStatus.Cancelled;
        Check.Fails(() => item.MoveTo(GlobalVendorReviewStatus.Planned), "move");
    }

    public static void RunAll()
    {
        GlobalVendorReviewStartsInPlanned();
        GlobalVendorReviewMovesToScheduled();
        GlobalVendorReviewRejectsMovesFromCancelled();
    }
}
