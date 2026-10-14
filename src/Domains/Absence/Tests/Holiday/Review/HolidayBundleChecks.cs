namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class HolidayBundleChecks
{
    public static void HolidayBundleStartsInOpen()
    {
        var item = HolidayBundle.NewHolidayBundle("code-853", "name-747");
        Check.Equal(item.Status, HolidayBundleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayBundleMovesToArchived()
    {
        var item = HolidayBundle.NewHolidayBundle("code-853", "name-747");
        try
        {
            item.MoveTo(HolidayBundleStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayBundleStatus.Archived, "status");
    }

    public static void HolidayBundleRejectsMovesFromArchived()
    {
        var item = HolidayBundle.NewHolidayBundle("code-853", "name-747");
        item.Status = HolidayBundleStatus.Archived;
        Check.Fails(() => item.MoveTo(HolidayBundleStatus.Open), "move");
    }

    public static void RunAll()
    {
        HolidayBundleStartsInOpen();
        HolidayBundleMovesToArchived();
        HolidayBundleRejectsMovesFromArchived();
    }
}
