namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled succession plan audit policy.</summary>
public static class ScheduledSuccessionPlanAuditPolicy
{
    public static List<string> MergeScheduledSuccessionPlanAuditLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 4230.0, in order.</summary>
    public static int ScheduledSuccessionPlanAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 4230.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a scheduled succession plan audit code in its canonical SSPA- form.</summary>
    public static string NormalizeScheduledSuccessionPlanAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SSPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SSPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a scheduled succession plan audit score falls in.</summary>
    public static string ClassifyScheduledSuccessionPlanAudit(double score)
    {
        if (score < 39.5)
        {
            return "watch";
        }
        if (score < 62.7)
        {
            return "medium";
        }
        if (score < 67.4)
        {
            return "minimal";
        }
        if (score < 90.7)
        {
            return "critical";
        }
        return "low";
    }
}
