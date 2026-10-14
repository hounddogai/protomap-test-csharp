namespace Acme.Workforce.Contractors.StatementOfWork.Segment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of statement of work segment policy.</summary>
public static class StatementOfWorkSegmentPolicy
{
    /// <summary>Puts a statement of work segment code in its canonical SOWS- form.</summary>
    public static string NormalizeStatementOfWorkSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SOWS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SOWS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeStatementOfWorkSegmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double StatementOfWorkSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.39;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a statement of work segment score falls in.</summary>
    public static string ClassifyStatementOfWorkSegment(double score)
    {
        if (score < 19.1)
        {
            return "ok";
        }
        if (score < 29.0)
        {
            return "high";
        }
        if (score < 69.8)
        {
            return "low";
        }
        if (score < 86.2)
        {
            return "medium";
        }
        return "minimal";
    }
}
