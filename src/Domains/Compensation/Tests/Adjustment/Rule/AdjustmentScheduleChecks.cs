namespace Acme.Workforce.Compensation.Tests.Adjustment.Rule;

using Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AdjustmentScheduleChecks
{
    public static void AdjustmentScheduleStartsInCreated()
    {
        var item = AdjustmentSchedule.NewAdjustmentSchedule("code-958", "label-869");
        Check.Equal(item.Status, AdjustmentScheduleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentScheduleMovesToRetired()
    {
        var item = AdjustmentSchedule.NewAdjustmentSchedule("code-958", "label-869");
        try
        {
            item.MoveTo(AdjustmentScheduleStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentScheduleStatus.Retired, "status");
    }

    public static void AdjustmentScheduleRejectsMovesFromRetired()
    {
        var item = AdjustmentSchedule.NewAdjustmentSchedule("code-958", "label-869");
        item.Status = AdjustmentScheduleStatus.Retired;
        Check.Fails(() => item.MoveTo(AdjustmentScheduleStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdjustmentScheduleStartsInCreated();
        AdjustmentScheduleMovesToRetired();
        AdjustmentScheduleRejectsMovesFromRetired();
    }
}
