namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathScheduleViewChecks
{
    public static void CareerPathScheduleViewShowsTheStatusInCapitals()
    {
        var item = CareerPathSchedule.NewCareerPathSchedule("code-56", "label-427");
        var view = CareerPathScheduleView.ToCareerPathScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CareerPathScheduleView.ToCareerPathScheduleViews(new List<CareerPathSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CareerPathScheduleViewShowsTheStatusInCapitals();
    }
}
