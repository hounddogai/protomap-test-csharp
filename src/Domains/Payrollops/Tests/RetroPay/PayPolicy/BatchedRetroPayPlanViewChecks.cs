namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayPolicy;

using Acme.Workforce.Payrollops.RetroPay.PayPolicy;

/// <summary>Checks of pay policy code, run by the project's runner.</summary>
public static class BatchedRetroPayPlanViewChecks
{
    public static void BatchedRetroPayPlanViewShowsTheStatusInCapitals()
    {
        var item = BatchedRetroPayPlan.NewBatchedRetroPayPlan("code-443", "name-50");
        var view = BatchedRetroPayPlanView.ToBatchedRetroPayPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedRetroPayPlanView.ToBatchedRetroPayPlanViews(new List<BatchedRetroPayPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedRetroPayPlanViewShowsTheStatusInCapitals();
    }
}
