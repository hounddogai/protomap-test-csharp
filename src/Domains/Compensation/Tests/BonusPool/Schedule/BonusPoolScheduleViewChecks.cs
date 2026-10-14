namespace Acme.Workforce.Compensation.Tests.BonusPool.Schedule;

using Acme.Workforce.Compensation.BonusPool.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class BonusPoolScheduleViewChecks
{
    public static void BonusPoolScheduleViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolSchedule.NewBonusPoolSchedule("code-576", "title-279");
        var view = BonusPoolScheduleView.ToBonusPoolScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolScheduleView.ToBonusPoolScheduleViews(new List<BonusPoolSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolScheduleViewShowsTheStatusInCapitals();
    }
}
