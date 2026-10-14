namespace Acme.Workforce.Absence.Tests.Accrual.Schedule;

using Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class PrimaryAccrualScheduleViewChecks
{
    public static void PrimaryAccrualScheduleViewShowsTheStatusInCapitals()
    {
        var item = PrimaryAccrualSchedule.NewPrimaryAccrualSchedule("code-131", "label-204");
        var view = PrimaryAccrualScheduleView.ToPrimaryAccrualScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryAccrualScheduleView.ToPrimaryAccrualScheduleViews(new List<PrimaryAccrualSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryAccrualScheduleViewShowsTheStatusInCapitals();
    }
}
