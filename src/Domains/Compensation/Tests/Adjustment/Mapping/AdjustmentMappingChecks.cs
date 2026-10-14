namespace Acme.Workforce.Compensation.Tests.Adjustment.Mapping;

using Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdjustmentMappingChecks
{
    public static void AdjustmentMappingStartsInPlanned()
    {
        var item = AdjustmentMapping.NewAdjustmentMapping("code-836", "title-188");
        Check.Equal(item.Status, AdjustmentMappingStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentMappingMovesToScheduled()
    {
        var item = AdjustmentMapping.NewAdjustmentMapping("code-836", "title-188");
        try
        {
            item.MoveTo(AdjustmentMappingStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentMappingStatus.Scheduled, "status");
    }

    public static void AdjustmentMappingRejectsMovesFromCancelled()
    {
        var item = AdjustmentMapping.NewAdjustmentMapping("code-836", "title-188");
        item.Status = AdjustmentMappingStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdjustmentMappingStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdjustmentMappingStartsInPlanned();
        AdjustmentMappingMovesToScheduled();
        AdjustmentMappingRejectsMovesFromCancelled();
    }
}
