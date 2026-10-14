namespace Acme.Workforce.Payrollops.Tests.RetroPay.Claim;

using Acme.Workforce.Payrollops.RetroPay.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AdaptiveRetroPayMetricChecks
{
    public static void AdaptiveRetroPayMetricStartsInCreated()
    {
        var item = AdaptiveRetroPayMetric.NewAdaptiveRetroPayMetric("code-219", "title-716");
        Check.Equal(item.Status, AdaptiveRetroPayMetricStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveRetroPayMetricMovesToValidated()
    {
        var item = AdaptiveRetroPayMetric.NewAdaptiveRetroPayMetric("code-219", "title-716");
        try
        {
            item.MoveTo(AdaptiveRetroPayMetricStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveRetroPayMetricStatus.Validated, "status");
    }

    public static void AdaptiveRetroPayMetricRejectsMovesFromRetired()
    {
        var item = AdaptiveRetroPayMetric.NewAdaptiveRetroPayMetric("code-219", "title-716");
        item.Status = AdaptiveRetroPayMetricStatus.Retired;
        Check.Fails(() => item.MoveTo(AdaptiveRetroPayMetricStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdaptiveRetroPayMetricStartsInCreated();
        AdaptiveRetroPayMetricMovesToValidated();
        AdaptiveRetroPayMetricRejectsMovesFromRetired();
    }
}
