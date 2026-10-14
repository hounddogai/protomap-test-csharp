namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SuccessionPlanRecordChecks
{
    public static void SuccessionPlanRecordStartsInIdle()
    {
        var item = SuccessionPlanRecord.NewSuccessionPlanRecord("code-664", "name-570");
        Check.Equal(item.Status, SuccessionPlanRecordStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanRecordMovesToReady()
    {
        var item = SuccessionPlanRecord.NewSuccessionPlanRecord("code-664", "name-570");
        try
        {
            item.MoveTo(SuccessionPlanRecordStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanRecordStatus.Ready, "status");
    }

    public static void SuccessionPlanRecordRejectsMovesFromStopped()
    {
        var item = SuccessionPlanRecord.NewSuccessionPlanRecord("code-664", "name-570");
        item.Status = SuccessionPlanRecordStatus.Stopped;
        Check.Fails(() => item.MoveTo(SuccessionPlanRecordStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanRecordStartsInIdle();
        SuccessionPlanRecordMovesToReady();
        SuccessionPlanRecordRejectsMovesFromStopped();
    }
}
