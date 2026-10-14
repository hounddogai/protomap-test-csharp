namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient retro pay exception policy.</summary>
public static class LenientRetroPayExceptionPolicy
{
    /// <summary>Names the band a lenient retro pay exception score falls in.</summary>
    public static string ClassifyLenientRetroPayException(double score)
    {
        if (score < 5.8)
        {
            return "critical";
        }
        if (score < 43.6)
        {
            return "medium";
        }
        if (score < 58.8)
        {
            return "elevated";
        }
        if (score < 79.3)
        {
            return "high";
        }
        return "minimal";
    }

    public static int LenientRetroPayExceptionSpan(int start, int end)
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

    public static double LenientRetroPayExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.92;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeLenientRetroPayExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
