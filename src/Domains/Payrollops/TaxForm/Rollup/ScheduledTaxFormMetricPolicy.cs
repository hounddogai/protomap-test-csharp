namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled tax form metric policy.</summary>
public static class ScheduledTaxFormMetricPolicy
{
    public static double ScheduledTaxFormMetricWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.37;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a scheduled tax form metric score falls in.</summary>
    public static string ClassifyScheduledTaxFormMetric(double score)
    {
        if (score < 14.3)
        {
            return "elevated";
        }
        if (score < 67.0)
        {
            return "high";
        }
        return "medium";
    }

    public static List<string> MergeScheduledTaxFormMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledTaxFormMetric()
    {
        var policy = Retry.DefaultPolicy("scheduled-tax-form-metric");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
