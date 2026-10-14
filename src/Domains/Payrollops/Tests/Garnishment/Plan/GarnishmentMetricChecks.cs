namespace Acme.Workforce.Payrollops.Tests.Garnishment.Plan;

using Acme.Workforce.Payrollops.Garnishment.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class GarnishmentMetricChecks
{
    public static void GarnishmentMetricStartsInNew()
    {
        var item = GarnishmentMetric.NewGarnishmentMetric("code-796", "name-986");
        Check.Equal(item.Status, GarnishmentMetricStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentMetricMovesToFailed()
    {
        var item = GarnishmentMetric.NewGarnishmentMetric("code-796", "name-986");
        try
        {
            item.MoveTo(GarnishmentMetricStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentMetricStatus.Failed, "status");
    }

    public static void GarnishmentMetricRejectsMovesFromFailed()
    {
        var item = GarnishmentMetric.NewGarnishmentMetric("code-796", "name-986");
        item.Status = GarnishmentMetricStatus.Failed;
        Check.Fails(() => item.MoveTo(GarnishmentMetricStatus.New), "move");
    }

    public static void RunAll()
    {
        GarnishmentMetricStartsInNew();
        GarnishmentMetricMovesToFailed();
        GarnishmentMetricRejectsMovesFromFailed();
    }
}
