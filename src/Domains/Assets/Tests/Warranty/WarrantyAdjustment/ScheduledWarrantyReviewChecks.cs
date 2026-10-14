namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

/// <summary>Checks of warranty adjustment code, run by the project's runner.</summary>
public static class ScheduledWarrantyReviewChecks
{
    public static void ScheduledWarrantyReviewStartsInPlanned()
    {
        var item = ScheduledWarrantyReview.NewScheduledWarrantyReview("code-452", "title-464");
        Check.Equal(item.Status, ScheduledWarrantyReviewStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledWarrantyReviewMovesToScheduled()
    {
        var item = ScheduledWarrantyReview.NewScheduledWarrantyReview("code-452", "title-464");
        try
        {
            item.MoveTo(ScheduledWarrantyReviewStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledWarrantyReviewStatus.Scheduled, "status");
    }

    public static void ScheduledWarrantyReviewRejectsMovesFromCancelled()
    {
        var item = ScheduledWarrantyReview.NewScheduledWarrantyReview("code-452", "title-464");
        item.Status = ScheduledWarrantyReviewStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ScheduledWarrantyReviewStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ScheduledWarrantyReviewStartsInPlanned();
        ScheduledWarrantyReviewMovesToScheduled();
        ScheduledWarrantyReviewRejectsMovesFromCancelled();
    }
}
