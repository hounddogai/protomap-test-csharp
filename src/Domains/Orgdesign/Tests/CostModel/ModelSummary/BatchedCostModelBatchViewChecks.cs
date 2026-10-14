namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelSummary;

using Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>Checks of model summary code, run by the project's runner.</summary>
public static class BatchedCostModelBatchViewChecks
{
    public static void BatchedCostModelBatchViewShowsTheStatusInCapitals()
    {
        var item = BatchedCostModelBatch.NewBatchedCostModelBatch("code-602", "title-675");
        var view = BatchedCostModelBatchView.ToBatchedCostModelBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedCostModelBatchView.ToBatchedCostModelBatchViews(new List<BatchedCostModelBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedCostModelBatchViewShowsTheStatusInCapitals();
    }
}
