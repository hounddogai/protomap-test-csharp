namespace Acme.Workforce.Payrollops.Tests.PayRun.Bundle;

using Acme.Workforce.Payrollops.PayRun.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PayRunBatchChecks
{
    public static void PayRunBatchStartsInPlanned()
    {
        var item = PayRunBatch.NewPayRunBatch("code-401", "label-180");
        Check.Equal(item.Status, PayRunBatchStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunBatchMovesToInProgress()
    {
        var item = PayRunBatch.NewPayRunBatch("code-401", "label-180");
        try
        {
            item.MoveTo(PayRunBatchStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunBatchStatus.InProgress, "status");
    }

    public static void PayRunBatchRejectsMovesFromCancelled()
    {
        var item = PayRunBatch.NewPayRunBatch("code-401", "label-180");
        item.Status = PayRunBatchStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PayRunBatchStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PayRunBatchStartsInPlanned();
        PayRunBatchMovesToInProgress();
        PayRunBatchRejectsMovesFromCancelled();
    }
}
