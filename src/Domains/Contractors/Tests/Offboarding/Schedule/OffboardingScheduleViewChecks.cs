namespace Acme.Workforce.Contractors.Tests.Offboarding.Schedule;

using Acme.Workforce.Contractors.Offboarding.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class OffboardingScheduleViewChecks
{
    public static void OffboardingScheduleViewShowsTheStatusInCapitals()
    {
        var item = OffboardingSchedule.NewOffboardingSchedule("code-441", "title-328");
        var view = OffboardingScheduleView.ToOffboardingScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OffboardingScheduleView.ToOffboardingScheduleViews(new List<OffboardingSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OffboardingScheduleViewShowsTheStatusInCapitals();
    }
}
