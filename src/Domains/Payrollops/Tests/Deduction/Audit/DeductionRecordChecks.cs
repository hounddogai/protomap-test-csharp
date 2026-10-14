namespace Acme.Workforce.Payrollops.Tests.Deduction.Audit;

using Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class DeductionRecordChecks
{
    public static void DeductionRecordStartsInIdle()
    {
        var item = DeductionRecord.NewDeductionRecord("code-606", "label-300");
        Check.Equal(item.Status, DeductionRecordStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionRecordMovesToDraining()
    {
        var item = DeductionRecord.NewDeductionRecord("code-606", "label-300");
        try
        {
            item.MoveTo(DeductionRecordStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionRecordStatus.Draining, "status");
    }

    public static void DeductionRecordRejectsMovesFromStopped()
    {
        var item = DeductionRecord.NewDeductionRecord("code-606", "label-300");
        item.Status = DeductionRecordStatus.Stopped;
        Check.Fails(() => item.MoveTo(DeductionRecordStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DeductionRecordStartsInIdle();
        DeductionRecordMovesToDraining();
        DeductionRecordRejectsMovesFromStopped();
    }
}
