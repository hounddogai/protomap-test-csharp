namespace Acme.Workforce.Engagement.Tests.ActionPlan.Batch;

using Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class TieredActionPlanBatchViewChecks
{
    public static void TieredActionPlanBatchViewShowsTheStatusInCapitals()
    {
        var item = TieredActionPlanBatch.NewTieredActionPlanBatch("code-836", "label-665");
        var view = TieredActionPlanBatchView.ToTieredActionPlanBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredActionPlanBatchView.ToTieredActionPlanBatchViews(new List<TieredActionPlanBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredActionPlanBatchViewShowsTheStatusInCapitals();
    }
}
