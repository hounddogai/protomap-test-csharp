namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled tax form forecast policy.</summary>
public static class ScheduledTaxFormForecastPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledTaxFormForecast()
    {
        var policy = Retry.DefaultPolicy("scheduled-tax-form-forecast");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a scheduled tax form forecast score falls in.</summary>
    public static string ClassifyScheduledTaxFormForecast(double score)
    {
        if (score < 28.4)
        {
            return "elevated";
        }
        if (score < 41.2)
        {
            return "watch";
        }
        if (score < 89.2)
        {
            return "critical";
        }
        return "ok";
    }

    public static int ScheduledTaxFormForecastSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }
}
