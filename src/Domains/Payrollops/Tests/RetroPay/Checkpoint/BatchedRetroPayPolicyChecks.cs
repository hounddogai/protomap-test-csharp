namespace Acme.Workforce.Payrollops.Tests.RetroPay.Checkpoint;

using Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class BatchedRetroPayPolicyChecks
{
    public static void BatchedRetroPayPolicyStartsInIdle()
    {
        var item = BatchedRetroPayPolicy.NewBatchedRetroPayPolicy("code-894", "title-96");
        Check.Equal(item.Status, BatchedRetroPayPolicyStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedRetroPayPolicyMovesToReady()
    {
        var item = BatchedRetroPayPolicy.NewBatchedRetroPayPolicy("code-894", "title-96");
        try
        {
            item.MoveTo(BatchedRetroPayPolicyStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedRetroPayPolicyStatus.Ready, "status");
    }

    public static void BatchedRetroPayPolicyRejectsMovesFromStopped()
    {
        var item = BatchedRetroPayPolicy.NewBatchedRetroPayPolicy("code-894", "title-96");
        item.Status = BatchedRetroPayPolicyStatus.Stopped;
        Check.Fails(() => item.MoveTo(BatchedRetroPayPolicyStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BatchedRetroPayPolicyStartsInIdle();
        BatchedRetroPayPolicyMovesToReady();
        BatchedRetroPayPolicyRejectsMovesFromStopped();
    }
}
