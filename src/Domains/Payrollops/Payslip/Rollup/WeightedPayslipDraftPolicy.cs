namespace Acme.Workforce.Payrollops.Payslip.Rollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted payslip draft policy.</summary>
public static class WeightedPayslipDraftPolicy
{
    /// <summary>Puts a weighted payslip draft code in its canonical WPD- form.</summary>
    public static string NormalizeWeightedPayslipDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WPD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WPD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a weighted payslip draft score falls in.</summary>
    public static string ClassifyWeightedPayslipDraft(double score)
    {
        if (score < 23.3)
        {
            return "watch";
        }
        if (score < 39.7)
        {
            return "elevated";
        }
        if (score < 54.4)
        {
            return "low";
        }
        if (score < 56.8)
        {
            return "critical";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 43821.0, in order.</summary>
    public static int WeightedPayslipDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 43821.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeWeightedPayslipDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
