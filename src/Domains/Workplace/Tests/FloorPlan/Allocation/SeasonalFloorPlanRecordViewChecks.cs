namespace Acme.Workforce.Workplace.Tests.FloorPlan.Allocation;

using Acme.Workforce.Workplace.FloorPlan.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class SeasonalFloorPlanRecordViewChecks
{
    public static void SeasonalFloorPlanRecordViewShowsTheStatusInCapitals()
    {
        var item = SeasonalFloorPlanRecord.NewSeasonalFloorPlanRecord("code-146", "label-742");
        var view = SeasonalFloorPlanRecordView.ToSeasonalFloorPlanRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalFloorPlanRecordView.ToSeasonalFloorPlanRecordViews(new List<SeasonalFloorPlanRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalFloorPlanRecordViewShowsTheStatusInCapitals();
    }
}
