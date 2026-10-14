namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual garnishment baseline policy.</summary>
public static class ManualGarnishmentBaselinePolicy
{
    public static List<string> MergeManualGarnishmentBaselineLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int ManualGarnishmentBaselineSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Puts a manual garnishment baseline code in its canonical MGB- form.</summary>
    public static string NormalizeManualGarnishmentBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MGB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MGB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 33902.0, in order.</summary>
    public static int ManualGarnishmentBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 33902.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
