namespace Acme.Workforce.Contractors.Tests.Rate.Window;

using Acme.Workforce.Contractors.Rate.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RateMetricChecks
{
    public static void RateMetricStartsInCreated()
    {
        var item = RateMetric.NewRateMetric("code-341", "name-658");
        Check.Equal(item.Status, RateMetricStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateMetricMovesToValidated()
    {
        var item = RateMetric.NewRateMetric("code-341", "name-658");
        try
        {
            item.MoveTo(RateMetricStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateMetricStatus.Validated, "status");
    }

    public static void RateMetricRejectsMovesFromRetired()
    {
        var item = RateMetric.NewRateMetric("code-341", "name-658");
        item.Status = RateMetricStatus.Retired;
        Check.Fails(() => item.MoveTo(RateMetricStatus.Created), "move");
    }

    public static void RunAll()
    {
        RateMetricStartsInCreated();
        RateMetricMovesToValidated();
        RateMetricRejectsMovesFromRetired();
    }
}
