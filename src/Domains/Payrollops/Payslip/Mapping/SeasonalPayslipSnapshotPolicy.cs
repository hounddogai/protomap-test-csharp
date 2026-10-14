namespace Acme.Workforce.Payrollops.Payslip.Mapping;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal payslip snapshot policy.</summary>
public static class SeasonalPayslipSnapshotPolicy
{
    public static List<string> MergeSeasonalPayslipSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string SeasonalPayslipSnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a seasonal payslip snapshot score falls in.</summary>
    public static string ClassifySeasonalPayslipSnapshot(double score)
    {
        if (score < 34.8)
        {
            return "high";
        }
        if (score < 44.1)
        {
            return "low";
        }
        if (score < 44.7)
        {
            return "ok";
        }
        if (score < 45.9)
        {
            return "critical";
        }
        return "minimal";
    }

    public static double SeasonalPayslipSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.62;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
