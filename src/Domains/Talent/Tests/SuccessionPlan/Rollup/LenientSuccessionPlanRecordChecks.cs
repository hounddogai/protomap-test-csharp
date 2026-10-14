namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Rollup;

using Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientSuccessionPlanRecordChecks
{
    public static void LenientSuccessionPlanRecordStartsInPlanned()
    {
        var item = LenientSuccessionPlanRecord.NewLenientSuccessionPlanRecord("code-53", "name-23");
        Check.Equal(item.Status, LenientSuccessionPlanRecordStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientSuccessionPlanRecordMovesToCompleted()
    {
        var item = LenientSuccessionPlanRecord.NewLenientSuccessionPlanRecord("code-53", "name-23");
        try
        {
            item.MoveTo(LenientSuccessionPlanRecordStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientSuccessionPlanRecordStatus.Completed, "status");
    }

    public static void LenientSuccessionPlanRecordRejectsMovesFromCancelled()
    {
        var item = LenientSuccessionPlanRecord.NewLenientSuccessionPlanRecord("code-53", "name-23");
        item.Status = LenientSuccessionPlanRecordStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientSuccessionPlanRecordStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientSuccessionPlanRecordStartsInPlanned();
        LenientSuccessionPlanRecordMovesToCompleted();
        LenientSuccessionPlanRecordRejectsMovesFromCancelled();
    }
}
