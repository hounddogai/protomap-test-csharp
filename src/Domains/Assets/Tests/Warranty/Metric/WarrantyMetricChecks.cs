namespace Acme.Workforce.Assets.Tests.Warranty.Metric;

using Acme.Workforce.Assets.Warranty.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class WarrantyMetricChecks
{
    public static void WarrantyMetricStartsInNew()
    {
        var item = WarrantyMetric.NewWarrantyMetric("code-625", "title-11");
        Check.Equal(item.Status, WarrantyMetricStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyMetricMovesToQueued()
    {
        var item = WarrantyMetric.NewWarrantyMetric("code-625", "title-11");
        try
        {
            item.MoveTo(WarrantyMetricStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyMetricStatus.Queued, "status");
    }

    public static void WarrantyMetricRejectsMovesFromFailed()
    {
        var item = WarrantyMetric.NewWarrantyMetric("code-625", "title-11");
        item.Status = WarrantyMetricStatus.Failed;
        Check.Fails(() => item.MoveTo(WarrantyMetricStatus.New), "move");
    }

    public static void RunAll()
    {
        WarrantyMetricStartsInNew();
        WarrantyMetricMovesToQueued();
        WarrantyMetricRejectsMovesFromFailed();
    }
}
