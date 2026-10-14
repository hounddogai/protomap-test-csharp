namespace Acme.Workforce.Payrollops.RetroPay.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of retro pay window policy.</summary>
public static class RetroPayWindowPolicy
{
    public static List<string> MergeRetroPayWindowLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a retro pay window score falls in.</summary>
    public static string ClassifyRetroPayWindow(double score)
    {
        if (score < 37.6)
        {
            return "elevated";
        }
        if (score < 67.9)
        {
            return "watch";
        }
        return "minimal";
    }

    public static int RetroPayWindowSpan(int start, int end)
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

    public static double RetroPayWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.04;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
