namespace Acme.Workforce.Compensation.Tests.Adjustment.Rule;

using Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AdjustmentScheduleViewChecks
{
    public static void AdjustmentScheduleViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentSchedule.NewAdjustmentSchedule("code-958", "label-869");
        var view = AdjustmentScheduleView.ToAdjustmentScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentScheduleView.ToAdjustmentScheduleViews(new List<AdjustmentSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentScheduleViewShowsTheStatusInCapitals();
    }
}
