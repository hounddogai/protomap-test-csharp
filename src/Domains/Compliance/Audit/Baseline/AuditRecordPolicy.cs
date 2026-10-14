namespace Acme.Workforce.Compliance.Audit.Baseline;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of audit record policy.</summary>
public static class AuditRecordPolicy
{
    public static List<string> MergeAuditRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 26673.0, in order.</summary>
    public static int AuditRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26673.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a audit record score falls in.</summary>
    public static string ClassifyAuditRecord(double score)
    {
        if (score < 44.8)
        {
            return "elevated";
        }
        if (score < 49.6)
        {
            return "watch";
        }
        if (score < 81.4)
        {
            return "minimal";
        }
        return "high";
    }

    public static int AuditRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }
}
