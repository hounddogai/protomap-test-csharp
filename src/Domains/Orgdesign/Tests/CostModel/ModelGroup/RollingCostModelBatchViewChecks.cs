namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelGroup;

using Acme.Workforce.Orgdesign.CostModel.ModelGroup;

/// <summary>Checks of model group code, run by the project's runner.</summary>
public static class RollingCostModelBatchViewChecks
{
    public static void RollingCostModelBatchViewShowsTheStatusInCapitals()
    {
        var item = RollingCostModelBatch.NewRollingCostModelBatch("code-378", "label-206");
        var view = RollingCostModelBatchView.ToRollingCostModelBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingCostModelBatchView.ToRollingCostModelBatchViews(new List<RollingCostModelBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingCostModelBatchViewShowsTheStatusInCapitals();
    }
}
