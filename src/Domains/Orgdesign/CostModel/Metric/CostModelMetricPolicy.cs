namespace Acme.Workforce.Orgdesign.CostModel.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of cost model metric policy.</summary>
public static class CostModelMetricPolicy
{
    public static int CostModelMetricSpan(int start, int end)
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

    public static double CostModelMetricWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.85;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string CostModelMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
