namespace Acme.Workforce.Contractors.Tests.Timesheet.OverrideItem;

using Acme.Workforce.Contractors.Timesheet.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class StrictTimesheetEstimateChecks
{
    public static void StrictTimesheetEstimateStartsInNew()
    {
        var item = StrictTimesheetEstimate.NewStrictTimesheetEstimate("code-332", "label-232");
        Check.Equal(item.Status, StrictTimesheetEstimateStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictTimesheetEstimateMovesToRunning()
    {
        var item = StrictTimesheetEstimate.NewStrictTimesheetEstimate("code-332", "label-232");
        try
        {
            item.MoveTo(StrictTimesheetEstimateStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictTimesheetEstimateStatus.Running, "status");
    }

    public static void StrictTimesheetEstimateRejectsMovesFromFailed()
    {
        var item = StrictTimesheetEstimate.NewStrictTimesheetEstimate("code-332", "label-232");
        item.Status = StrictTimesheetEstimateStatus.Failed;
        Check.Fails(() => item.MoveTo(StrictTimesheetEstimateStatus.New), "move");
    }

    public static void RunAll()
    {
        StrictTimesheetEstimateStartsInNew();
        StrictTimesheetEstimateMovesToRunning();
        StrictTimesheetEstimateRejectsMovesFromFailed();
    }
}
