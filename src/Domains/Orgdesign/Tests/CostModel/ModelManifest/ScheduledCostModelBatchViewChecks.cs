namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelManifest;

using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Checks of model manifest code, run by the project's runner.</summary>
public static class ScheduledCostModelBatchViewChecks
{
    public static void ScheduledCostModelBatchViewShowsTheStatusInCapitals()
    {
        var item = ScheduledCostModelBatch.NewScheduledCostModelBatch("code-855", "name-74");
        var view = ScheduledCostModelBatchView.ToScheduledCostModelBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledCostModelBatchView.ToScheduledCostModelBatchViews(new List<ScheduledCostModelBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledCostModelBatchViewShowsTheStatusInCapitals();
    }
}
