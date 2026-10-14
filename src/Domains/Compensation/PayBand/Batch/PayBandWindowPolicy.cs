namespace Acme.Workforce.Compensation.PayBand.Batch;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pay band window policy.</summary>
public static class PayBandWindowPolicy
{
    /// <summary>Names the band a pay band window score falls in.</summary>
    public static string ClassifyPayBandWindow(double score)
    {
        if (score < 79.3)
        {
            return "minimal";
        }
        if (score < 83.3)
        {
            return "watch";
        }
        return "elevated";
    }

    public static double PayBandWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.27;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int PayBandWindowSpan(int start, int end)
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
        return span / 3600;
    }

    public static List<string> MergePayBandWindowLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
