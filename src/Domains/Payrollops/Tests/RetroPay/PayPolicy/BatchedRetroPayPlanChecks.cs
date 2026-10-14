namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayPolicy;

using Acme.Workforce.Payrollops.RetroPay.PayPolicy;

/// <summary>Checks of pay policy code, run by the project's runner.</summary>
public static class BatchedRetroPayPlanChecks
{
    public static void BatchedRetroPayPlanStartsInIdle()
    {
        var item = BatchedRetroPayPlan.NewBatchedRetroPayPlan("code-443", "name-50");
        Check.Equal(item.Status, BatchedRetroPayPlanStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedRetroPayPlanMovesToWarming()
    {
        var item = BatchedRetroPayPlan.NewBatchedRetroPayPlan("code-443", "name-50");
        try
        {
            item.MoveTo(BatchedRetroPayPlanStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedRetroPayPlanStatus.Warming, "status");
    }

    public static void BatchedRetroPayPlanRejectsMovesFromStopped()
    {
        var item = BatchedRetroPayPlan.NewBatchedRetroPayPlan("code-443", "name-50");
        item.Status = BatchedRetroPayPlanStatus.Stopped;
        Check.Fails(() => item.MoveTo(BatchedRetroPayPlanStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BatchedRetroPayPlanStartsInIdle();
        BatchedRetroPayPlanMovesToWarming();
        BatchedRetroPayPlanRejectsMovesFromStopped();
    }
}
