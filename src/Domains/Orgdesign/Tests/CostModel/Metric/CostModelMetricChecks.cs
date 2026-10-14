namespace Acme.Workforce.Orgdesign.Tests.CostModel.Metric;

using Acme.Workforce.Orgdesign.CostModel.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CostModelMetricChecks
{
    public static void CostModelMetricStartsInCreated()
    {
        var item = CostModelMetric.NewCostModelMetric("code-604", "name-975");
        Check.Equal(item.Status, CostModelMetricStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelMetricMovesToRetired()
    {
        var item = CostModelMetric.NewCostModelMetric("code-604", "name-975");
        try
        {
            item.MoveTo(CostModelMetricStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelMetricStatus.Retired, "status");
    }

    public static void CostModelMetricRejectsMovesFromRetired()
    {
        var item = CostModelMetric.NewCostModelMetric("code-604", "name-975");
        item.Status = CostModelMetricStatus.Retired;
        Check.Fails(() => item.MoveTo(CostModelMetricStatus.Created), "move");
    }

    public static void RunAll()
    {
        CostModelMetricStartsInCreated();
        CostModelMetricMovesToRetired();
        CostModelMetricRejectsMovesFromRetired();
    }
}
