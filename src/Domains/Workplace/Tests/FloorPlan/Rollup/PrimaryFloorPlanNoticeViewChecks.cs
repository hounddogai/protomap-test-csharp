namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rollup;

using Acme.Workforce.Workplace.FloorPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryFloorPlanNoticeViewChecks
{
    public static void PrimaryFloorPlanNoticeViewShowsTheStatusInCapitals()
    {
        var item = PrimaryFloorPlanNotice.NewPrimaryFloorPlanNotice("code-569", "label-686");
        var view = PrimaryFloorPlanNoticeView.ToPrimaryFloorPlanNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryFloorPlanNoticeView.ToPrimaryFloorPlanNoticeViews(new List<PrimaryFloorPlanNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryFloorPlanNoticeViewShowsTheStatusInCapitals();
    }
}
