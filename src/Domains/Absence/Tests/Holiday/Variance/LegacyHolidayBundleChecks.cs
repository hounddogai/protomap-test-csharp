namespace Acme.Workforce.Absence.Tests.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class LegacyHolidayBundleChecks
{
    public static void LegacyHolidayBundleStartsInOpen()
    {
        var item = LegacyHolidayBundle.NewLegacyHolidayBundle("code-592", "title-338");
        Check.Equal(item.Status, LegacyHolidayBundleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyHolidayBundleMovesToArchived()
    {
        var item = LegacyHolidayBundle.NewLegacyHolidayBundle("code-592", "title-338");
        try
        {
            item.MoveTo(LegacyHolidayBundleStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyHolidayBundleStatus.Archived, "status");
    }

    public static void LegacyHolidayBundleRejectsMovesFromArchived()
    {
        var item = LegacyHolidayBundle.NewLegacyHolidayBundle("code-592", "title-338");
        item.Status = LegacyHolidayBundleStatus.Archived;
        Check.Fails(() => item.MoveTo(LegacyHolidayBundleStatus.Open), "move");
    }

    public static void RunAll()
    {
        LegacyHolidayBundleStartsInOpen();
        LegacyHolidayBundleMovesToArchived();
        LegacyHolidayBundleRejectsMovesFromArchived();
    }
}
