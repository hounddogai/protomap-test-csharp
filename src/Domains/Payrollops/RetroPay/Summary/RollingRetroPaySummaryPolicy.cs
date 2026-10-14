namespace Acme.Workforce.Payrollops.RetroPay.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling retro pay summary policy.</summary>
public static class RollingRetroPaySummaryPolicy
{
    public static double RollingRetroPaySummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.6;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int RollingRetroPaySummarySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a rolling retro pay summary score falls in.</summary>
    public static string ClassifyRollingRetroPaySummary(double score)
    {
        if (score < 17.7)
        {
            return "medium";
        }
        if (score < 26.7)
        {
            return "high";
        }
        if (score < 48.4)
        {
            return "low";
        }
        if (score < 83.1)
        {
            return "ok";
        }
        return "watch";
    }

    public static List<string> MergeRollingRetroPaySummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
