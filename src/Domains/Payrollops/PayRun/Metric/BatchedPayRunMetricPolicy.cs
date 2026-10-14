namespace Acme.Workforce.Payrollops.PayRun.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched pay run metric policy.</summary>
public static class BatchedPayRunMetricPolicy
{
    public static double BatchedPayRunMetricWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.69;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int BatchedPayRunMetricSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }
}
