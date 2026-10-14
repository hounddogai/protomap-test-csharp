namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SuccessionPlanBatchViewChecks
{
    public static void SuccessionPlanBatchViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanBatch.NewSuccessionPlanBatch("code-236", "title-560");
        var view = SuccessionPlanBatchView.ToSuccessionPlanBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanBatchView.ToSuccessionPlanBatchViews(new List<SuccessionPlanBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanBatchViewShowsTheStatusInCapitals();
    }
}
