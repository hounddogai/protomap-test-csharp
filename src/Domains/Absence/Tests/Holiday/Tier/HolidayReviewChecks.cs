namespace Acme.Workforce.Absence.Tests.Holiday.Tier;

using Acme.Workforce.Absence.Holiday.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HolidayReviewChecks
{
    public static void HolidayReviewStartsInIdle()
    {
        var item = HolidayReview.NewHolidayReview("code-158", "title-463");
        Check.Equal(item.Status, HolidayReviewStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayReviewMovesToWarming()
    {
        var item = HolidayReview.NewHolidayReview("code-158", "title-463");
        try
        {
            item.MoveTo(HolidayReviewStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayReviewStatus.Warming, "status");
    }

    public static void HolidayReviewRejectsMovesFromStopped()
    {
        var item = HolidayReview.NewHolidayReview("code-158", "title-463");
        item.Status = HolidayReviewStatus.Stopped;
        Check.Fails(() => item.MoveTo(HolidayReviewStatus.Idle), "move");
    }

    public static void RunAll()
    {
        HolidayReviewStartsInIdle();
        HolidayReviewMovesToWarming();
        HolidayReviewRejectsMovesFromStopped();
    }
}
