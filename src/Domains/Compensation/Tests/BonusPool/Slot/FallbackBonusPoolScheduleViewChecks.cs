namespace Acme.Workforce.Compensation.Tests.BonusPool.Slot;

using Acme.Workforce.Compensation.BonusPool.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class FallbackBonusPoolScheduleViewChecks
{
    public static void FallbackBonusPoolScheduleViewShowsTheStatusInCapitals()
    {
        var item = FallbackBonusPoolSchedule.NewFallbackBonusPoolSchedule("code-540", "name-995");
        var view = FallbackBonusPoolScheduleView.ToFallbackBonusPoolScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackBonusPoolScheduleView.ToFallbackBonusPoolScheduleViews(new List<FallbackBonusPoolSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackBonusPoolScheduleViewShowsTheStatusInCapitals();
    }
}
