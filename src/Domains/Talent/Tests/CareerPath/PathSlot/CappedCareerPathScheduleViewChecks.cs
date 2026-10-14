namespace Acme.Workforce.Talent.Tests.CareerPath.PathSlot;

using Acme.Workforce.Talent.CareerPath.PathSlot;

/// <summary>Checks of path slot code, run by the project's runner.</summary>
public static class CappedCareerPathScheduleViewChecks
{
    public static void CappedCareerPathScheduleViewShowsTheStatusInCapitals()
    {
        var item = CappedCareerPathSchedule.NewCappedCareerPathSchedule("code-695", "label-270");
        var view = CappedCareerPathScheduleView.ToCappedCareerPathScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedCareerPathScheduleView.ToCappedCareerPathScheduleViews(new List<CappedCareerPathSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedCareerPathScheduleViewShowsTheStatusInCapitals();
    }
}
