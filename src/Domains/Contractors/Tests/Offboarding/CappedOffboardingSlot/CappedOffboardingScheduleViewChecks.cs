namespace Acme.Workforce.Contractors.Tests.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

/// <summary>Checks of capped offboarding slot code, run by the project's runner.</summary>
public static class CappedOffboardingScheduleViewChecks
{
    public static void CappedOffboardingScheduleViewShowsTheStatusInCapitals()
    {
        var item = CappedOffboardingSchedule.NewCappedOffboardingSchedule("code-80", "label-632");
        var view = CappedOffboardingScheduleView.ToCappedOffboardingScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedOffboardingScheduleView.ToCappedOffboardingScheduleViews(new List<CappedOffboardingSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedOffboardingScheduleViewShowsTheStatusInCapitals();
    }
}
