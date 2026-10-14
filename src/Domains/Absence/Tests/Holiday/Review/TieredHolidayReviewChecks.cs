namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TieredHolidayReviewChecks
{
    public static void TieredHolidayReviewStartsInRequested()
    {
        var item = TieredHolidayReview.NewTieredHolidayReview("code-67", "name-584");
        Check.Equal(item.Status, TieredHolidayReviewStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredHolidayReviewMovesToGranted()
    {
        var item = TieredHolidayReview.NewTieredHolidayReview("code-67", "name-584");
        try
        {
            item.MoveTo(TieredHolidayReviewStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredHolidayReviewStatus.Granted, "status");
    }

    public static void TieredHolidayReviewRejectsMovesFromExpired()
    {
        var item = TieredHolidayReview.NewTieredHolidayReview("code-67", "name-584");
        item.Status = TieredHolidayReviewStatus.Expired;
        Check.Fails(() => item.MoveTo(TieredHolidayReviewStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TieredHolidayReviewStartsInRequested();
        TieredHolidayReviewMovesToGranted();
        TieredHolidayReviewRejectsMovesFromExpired();
    }
}
