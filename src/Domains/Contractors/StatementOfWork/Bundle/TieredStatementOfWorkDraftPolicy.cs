namespace Acme.Workforce.Contractors.StatementOfWork.Bundle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered statement of work draft policy.</summary>
public static class TieredStatementOfWorkDraftPolicy
{
    /// <summary>Names the band a tiered statement of work draft score falls in.</summary>
    public static string ClassifyTieredStatementOfWorkDraft(double score)
    {
        if (score < 41.7)
        {
            return "minimal";
        }
        if (score < 73.3)
        {
            return "low";
        }
        if (score < 76.2)
        {
            return "ok";
        }
        if (score < 92.0)
        {
            return "high";
        }
        return "medium";
    }

    public static List<string> MergeTieredStatementOfWorkDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double TieredStatementOfWorkDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.84;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string TieredStatementOfWorkDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
