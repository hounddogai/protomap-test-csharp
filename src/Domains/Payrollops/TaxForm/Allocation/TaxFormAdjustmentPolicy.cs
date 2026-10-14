namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tax form adjustment policy.</summary>
public static class TaxFormAdjustmentPolicy
{
    /// <summary>Puts a tax form adjustment code in its canonical TFA- form.</summary>
    public static string NormalizeTaxFormAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TFA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TFA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeTaxFormAdjustmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 28578.0, in order.</summary>
    public static int TaxFormAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 28578.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
