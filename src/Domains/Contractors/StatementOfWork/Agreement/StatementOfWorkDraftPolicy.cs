namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of statement of work draft policy.</summary>
public static class StatementOfWorkDraftPolicy
{
    public static List<string> MergeStatementOfWorkDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a statement of work draft code in its canonical SOWD- form.</summary>
    public static string NormalizeStatementOfWorkDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SOWD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SOWD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a statement of work draft score falls in.</summary>
    public static string ClassifyStatementOfWorkDraft(double score)
    {
        if (score < 14.0)
        {
            return "ok";
        }
        if (score < 26.5)
        {
            return "critical";
        }
        if (score < 47.2)
        {
            return "low";
        }
        return "elevated";
    }

    public static double StatementOfWorkDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.4;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
