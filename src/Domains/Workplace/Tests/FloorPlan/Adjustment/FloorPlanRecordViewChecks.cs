namespace Acme.Workforce.Workplace.Tests.FloorPlan.Adjustment;

using Acme.Workforce.Workplace.FloorPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FloorPlanRecordViewChecks
{
    public static void FloorPlanRecordViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanRecord.NewFloorPlanRecord("code-632", "name-522");
        var view = FloorPlanRecordView.ToFloorPlanRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanRecordView.ToFloorPlanRecordViews(new List<FloorPlanRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanRecordViewShowsTheStatusInCapitals();
    }
}
