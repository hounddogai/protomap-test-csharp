namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReturnPlanBatchViewChecks
{
    public static void ReturnPlanBatchViewShowsTheStatusInCapitals()
    {
        var item = ReturnPlanBatch.NewReturnPlanBatch("code-321", "label-494");
        var view = ReturnPlanBatchView.ToReturnPlanBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReturnPlanBatchView.ToReturnPlanBatchViews(new List<ReturnPlanBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReturnPlanBatchViewShowsTheStatusInCapitals();
    }
}
