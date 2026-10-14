namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of statement of work quota policy.</summary>
public static class StatementOfWorkQuotaPolicy
{
    public static List<string> MergeStatementOfWorkQuotaLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int StatementOfWorkQuotaSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Puts a statement of work quota code in its canonical SOWQ- form.</summary>
    public static string NormalizeStatementOfWorkQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SOWQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SOWQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
