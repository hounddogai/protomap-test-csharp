namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of statement of work mapping policy.</summary>
public static class StatementOfWorkMappingPolicy
{
    public static double StatementOfWorkMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.84;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeStatementOfWorkMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a statement of work mapping score falls in.</summary>
    public static string ClassifyStatementOfWorkMapping(double score)
    {
        if (score < 13.7)
        {
            return "minimal";
        }
        if (score < 33.1)
        {
            return "ok";
        }
        if (score < 64.7)
        {
            return "watch";
        }
        return "critical";
    }
}
