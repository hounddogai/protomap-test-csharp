namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rollup;

using Acme.Workforce.Workplace.FloorPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryFloorPlanNoticeChecks
{
    public static void PrimaryFloorPlanNoticeStartsInDraft()
    {
        var item = PrimaryFloorPlanNotice.NewPrimaryFloorPlanNotice("code-569", "label-686");
        Check.Equal(item.Status, PrimaryFloorPlanNoticeStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryFloorPlanNoticeMovesToSuspended()
    {
        var item = PrimaryFloorPlanNotice.NewPrimaryFloorPlanNotice("code-569", "label-686");
        try
        {
            item.MoveTo(PrimaryFloorPlanNoticeStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryFloorPlanNoticeStatus.Suspended, "status");
    }

    public static void PrimaryFloorPlanNoticeRejectsMovesFromClosed()
    {
        var item = PrimaryFloorPlanNotice.NewPrimaryFloorPlanNotice("code-569", "label-686");
        item.Status = PrimaryFloorPlanNoticeStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryFloorPlanNoticeStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryFloorPlanNoticeStartsInDraft();
        PrimaryFloorPlanNoticeMovesToSuspended();
        PrimaryFloorPlanNoticeRejectsMovesFromClosed();
    }
}
