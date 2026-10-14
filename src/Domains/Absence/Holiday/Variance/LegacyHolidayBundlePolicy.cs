namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy holiday bundle policy.</summary>
public static class LegacyHolidayBundlePolicy
{
    public static List<string> MergeLegacyHolidayBundleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double LegacyHolidayBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.51;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a legacy holiday bundle score falls in.</summary>
    public static string ClassifyLegacyHolidayBundle(double score)
    {
        if (score < 21.6)
        {
            return "high";
        }
        if (score < 98.9)
        {
            return "minimal";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyHolidayBundle()
    {
        var policy = Retry.DefaultPolicy("legacy-holiday-bundle");
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
}
