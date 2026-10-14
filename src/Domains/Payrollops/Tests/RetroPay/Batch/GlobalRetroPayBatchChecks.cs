namespace Acme.Workforce.Payrollops.Tests.RetroPay.Batch;

using Acme.Workforce.Payrollops.RetroPay.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class GlobalRetroPayBatchChecks
{
    public static void GlobalRetroPayBatchStartsInPlanned()
    {
        var item = GlobalRetroPayBatch.NewGlobalRetroPayBatch("code-372", "name-187");
        Check.Equal(item.Status, GlobalRetroPayBatchStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalRetroPayBatchMovesToScheduled()
    {
        var item = GlobalRetroPayBatch.NewGlobalRetroPayBatch("code-372", "name-187");
        try
        {
            item.MoveTo(GlobalRetroPayBatchStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalRetroPayBatchStatus.Scheduled, "status");
    }

    public static void GlobalRetroPayBatchRejectsMovesFromCancelled()
    {
        var item = GlobalRetroPayBatch.NewGlobalRetroPayBatch("code-372", "name-187");
        item.Status = GlobalRetroPayBatchStatus.Cancelled;
        Check.Fails(() => item.MoveTo(GlobalRetroPayBatchStatus.Planned), "move");
    }

    public static void RunAll()
    {
        GlobalRetroPayBatchStartsInPlanned();
        GlobalRetroPayBatchMovesToScheduled();
        GlobalRetroPayBatchRejectsMovesFromCancelled();
    }
}
