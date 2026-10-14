namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyFloorPlanThresholdChecks
{
    public static void LegacyFloorPlanThresholdStartsInRequested()
    {
        var item = LegacyFloorPlanThreshold.NewLegacyFloorPlanThreshold("code-663", "title-910");
        Check.Equal(item.Status, LegacyFloorPlanThresholdStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyFloorPlanThresholdMovesToRevoked()
    {
        var item = LegacyFloorPlanThreshold.NewLegacyFloorPlanThreshold("code-663", "title-910");
        try
        {
            item.MoveTo(LegacyFloorPlanThresholdStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyFloorPlanThresholdStatus.Revoked, "status");
    }

    public static void LegacyFloorPlanThresholdRejectsMovesFromExpired()
    {
        var item = LegacyFloorPlanThreshold.NewLegacyFloorPlanThreshold("code-663", "title-910");
        item.Status = LegacyFloorPlanThresholdStatus.Expired;
        Check.Fails(() => item.MoveTo(LegacyFloorPlanThresholdStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LegacyFloorPlanThresholdStartsInRequested();
        LegacyFloorPlanThresholdMovesToRevoked();
        LegacyFloorPlanThresholdRejectsMovesFromExpired();
    }
}
