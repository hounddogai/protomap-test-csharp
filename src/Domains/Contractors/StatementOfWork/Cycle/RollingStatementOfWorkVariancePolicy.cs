namespace Acme.Workforce.Contractors.StatementOfWork.Cycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling statement of work variance policy.</summary>
public static class RollingStatementOfWorkVariancePolicy
{
    public static List<string> MergeRollingStatementOfWorkVarianceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a rolling statement of work variance code in its canonical RSOWV- form.</summary>
    public static string NormalizeRollingStatementOfWorkVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RSOWV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RSOWV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
