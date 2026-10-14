namespace Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped statement of work assignment policy.</summary>
public static class CappedStatementOfWorkAssignmentPolicy
{
    public static List<string> MergeCappedStatementOfWorkAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a capped statement of work assignment code in its canonical CSOWA- form.</summary>
    public static string NormalizeCappedStatementOfWorkAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CSOWA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CSOWA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
