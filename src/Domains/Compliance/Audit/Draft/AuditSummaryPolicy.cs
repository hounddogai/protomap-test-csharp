namespace Acme.Workforce.Compliance.Audit.Draft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of audit summary policy.</summary>
public static class AuditSummaryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 26315.0, in order.</summary>
    public static int AuditSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26315.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a audit summary code in its canonical AS- form.</summary>
    public static string NormalizeAuditSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeAuditSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
