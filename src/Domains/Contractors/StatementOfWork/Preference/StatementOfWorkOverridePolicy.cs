namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of statement of work override policy.</summary>
public static class StatementOfWorkOverridePolicy
{
    /// <summary>Names the band a statement of work override score falls in.</summary>
    public static string ClassifyStatementOfWorkOverride(double score)
    {
        if (score < 18.9)
        {
            return "low";
        }
        if (score < 50.0)
        {
            return "medium";
        }
        if (score < 64.0)
        {
            return "high";
        }
        return "critical";
    }

    public static List<string> MergeStatementOfWorkOverrideLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string StatementOfWorkOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
