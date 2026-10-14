namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled benchmark schedule policy.</summary>
public static class ScheduledBenchmarkSchedulePolicy
{
    /// <summary>Puts a scheduled benchmark schedule code in its canonical SBS- form.</summary>
    public static string NormalizeScheduledBenchmarkScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SBS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SBS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeScheduledBenchmarkScheduleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
