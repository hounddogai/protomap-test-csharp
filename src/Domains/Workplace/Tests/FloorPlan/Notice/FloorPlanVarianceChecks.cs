namespace Acme.Workforce.Workplace.Tests.FloorPlan.Notice;

using Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class FloorPlanVarianceChecks
{
    public static void FloorPlanVarianceStartsInOpen()
    {
        var item = FloorPlanVariance.NewFloorPlanVariance("code-844", "label-731");
        Check.Equal(item.Status, FloorPlanVarianceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanVarianceMovesToInReview()
    {
        var item = FloorPlanVariance.NewFloorPlanVariance("code-844", "label-731");
        try
        {
            item.MoveTo(FloorPlanVarianceStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanVarianceStatus.InReview, "status");
    }

    public static void FloorPlanVarianceRejectsMovesFromArchived()
    {
        var item = FloorPlanVariance.NewFloorPlanVariance("code-844", "label-731");
        item.Status = FloorPlanVarianceStatus.Archived;
        Check.Fails(() => item.MoveTo(FloorPlanVarianceStatus.Open), "move");
    }

    public static void RunAll()
    {
        FloorPlanVarianceStartsInOpen();
        FloorPlanVarianceMovesToInReview();
        FloorPlanVarianceRejectsMovesFromArchived();
    }
}
