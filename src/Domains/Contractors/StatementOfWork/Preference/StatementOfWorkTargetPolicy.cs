namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of statement of work target policy.</summary>
public static class StatementOfWorkTargetPolicy
{
    /// <summary>Names the band a statement of work target score falls in.</summary>
    public static string ClassifyStatementOfWorkTarget(double score)
    {
        if (score < 18.7)
        {
            return "critical";
        }
        if (score < 48.7)
        {
            return "minimal";
        }
        if (score < 97.9)
        {
            return "ok";
        }
        return "elevated";
    }

    public static string StatementOfWorkTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static List<string> MergeStatementOfWorkTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int StatementOfWorkTargetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}
