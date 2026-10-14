namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of deduction group policy.</summary>
public static class DeductionGroupPolicy
{
    /// <summary>Puts a deduction group code in its canonical DG- form.</summary>
    public static string NormalizeDeductionGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeDeductionGroupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 42565.0, in order.</summary>
    public static int DeductionGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 42565.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
