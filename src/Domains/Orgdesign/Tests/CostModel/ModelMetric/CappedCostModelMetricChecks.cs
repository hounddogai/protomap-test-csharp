namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelMetric;

using Acme.Workforce.Orgdesign.CostModel.ModelMetric;

/// <summary>Checks of model metric code, run by the project's runner.</summary>
public static class CappedCostModelMetricChecks
{
    public static void CappedCostModelMetricStartsInPlanned()
    {
        var item = CappedCostModelMetric.NewCappedCostModelMetric("code-851", "label-483");
        Check.Equal(item.Status, CappedCostModelMetricStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedCostModelMetricMovesToCompleted()
    {
        var item = CappedCostModelMetric.NewCappedCostModelMetric("code-851", "label-483");
        try
        {
            item.MoveTo(CappedCostModelMetricStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedCostModelMetricStatus.Completed, "status");
    }

    public static void CappedCostModelMetricRejectsMovesFromCancelled()
    {
        var item = CappedCostModelMetric.NewCappedCostModelMetric("code-851", "label-483");
        item.Status = CappedCostModelMetricStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CappedCostModelMetricStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CappedCostModelMetricStartsInPlanned();
        CappedCostModelMetricMovesToCompleted();
        CappedCostModelMetricRejectsMovesFromCancelled();
    }
}
