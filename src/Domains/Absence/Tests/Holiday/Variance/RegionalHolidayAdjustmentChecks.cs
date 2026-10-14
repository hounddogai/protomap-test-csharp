namespace Acme.Workforce.Absence.Tests.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RegionalHolidayAdjustmentChecks
{
    public static void RegionalHolidayAdjustmentStartsInNew()
    {
        var item = RegionalHolidayAdjustment.NewRegionalHolidayAdjustment("code-458", "title-330");
        Check.Equal(item.Status, RegionalHolidayAdjustmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalHolidayAdjustmentMovesToDone()
    {
        var item = RegionalHolidayAdjustment.NewRegionalHolidayAdjustment("code-458", "title-330");
        try
        {
            item.MoveTo(RegionalHolidayAdjustmentStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalHolidayAdjustmentStatus.Done, "status");
    }

    public static void RegionalHolidayAdjustmentRejectsMovesFromFailed()
    {
        var item = RegionalHolidayAdjustment.NewRegionalHolidayAdjustment("code-458", "title-330");
        item.Status = RegionalHolidayAdjustmentStatus.Failed;
        Check.Fails(() => item.MoveTo(RegionalHolidayAdjustmentStatus.New), "move");
    }

    public static void RunAll()
    {
        RegionalHolidayAdjustmentStartsInNew();
        RegionalHolidayAdjustmentMovesToDone();
        RegionalHolidayAdjustmentRejectsMovesFromFailed();
    }
}
