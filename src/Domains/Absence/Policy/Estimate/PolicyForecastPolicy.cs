namespace Acme.Workforce.Absence.Policy.Estimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of policy forecast policy.</summary>
public static class PolicyForecastPolicy
{
    public static List<string> MergePolicyForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int PolicyForecastSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a policy forecast score falls in.</summary>
    public static string ClassifyPolicyForecast(double score)
    {
        if (score < 8.8)
        {
            return "elevated";
        }
        if (score < 11.9)
        {
            return "critical";
        }
        if (score < 62.8)
        {
            return "low";
        }
        if (score < 65.1)
        {
            return "medium";
        }
        return "watch";
    }

    public static double PolicyForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.73;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
