namespace Acme.Workforce.Payrollops.Tests.RetroPay.Checkpoint;

using Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class BatchedRetroPayPolicyViewChecks
{
    public static void BatchedRetroPayPolicyViewShowsTheStatusInCapitals()
    {
        var item = BatchedRetroPayPolicy.NewBatchedRetroPayPolicy("code-894", "title-96");
        var view = BatchedRetroPayPolicyView.ToBatchedRetroPayPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedRetroPayPolicyView.ToBatchedRetroPayPolicyViews(new List<BatchedRetroPayPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedRetroPayPolicyViewShowsTheStatusInCapitals();
    }
}
