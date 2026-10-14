namespace Acme.Workforce.Workplace.Tests.FloorPlan.Baseline;

using Acme.Workforce.Workplace.FloorPlan.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DefaultFloorPlanBaselineChecks
{
    public static void DefaultFloorPlanBaselineStartsInDraft()
    {
        var item = DefaultFloorPlanBaseline.NewDefaultFloorPlanBaseline("code-376", "label-73");
        Check.Equal(item.Status, DefaultFloorPlanBaselineStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultFloorPlanBaselineMovesToPending()
    {
        var item = DefaultFloorPlanBaseline.NewDefaultFloorPlanBaseline("code-376", "label-73");
        try
        {
            item.MoveTo(DefaultFloorPlanBaselineStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultFloorPlanBaselineStatus.Pending, "status");
    }

    public static void DefaultFloorPlanBaselineRejectsMovesFromClosed()
    {
        var item = DefaultFloorPlanBaseline.NewDefaultFloorPlanBaseline("code-376", "label-73");
        item.Status = DefaultFloorPlanBaselineStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultFloorPlanBaselineStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultFloorPlanBaselineStartsInDraft();
        DefaultFloorPlanBaselineMovesToPending();
        DefaultFloorPlanBaselineRejectsMovesFromClosed();
    }
}
