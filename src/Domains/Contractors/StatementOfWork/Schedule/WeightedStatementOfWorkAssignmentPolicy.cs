namespace Acme.Workforce.Contractors.StatementOfWork.Schedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted statement of work assignment policy.</summary>
public static class WeightedStatementOfWorkAssignmentPolicy
{
    /// <summary>Puts a weighted statement of work assignment code in its canonical WSOWA- form.</summary>
    public static string NormalizeWeightedStatementOfWorkAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WSOWA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WSOWA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeWeightedStatementOfWorkAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int WeightedStatementOfWorkAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }
}
